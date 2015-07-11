module App

#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"

open System
open System.IO
open Suave.Http
open Suave.Http.Applicatives
open Suave.Utils.Collections
open Suave.Types
open Newtonsoft.Json

// Types

type ImageData = { name: string; url: string}
type ChoiceResponse = { correct: bool; message: string }

// Paths

let staticFilesRoot = Files.resolvePath __SOURCE_DIRECTORY__ "static"
let imagePath = Files.resolvePath staticFilesRoot "images"

// Startup

let images =
    Directory.GetFiles imagePath
    |> Array.map Path.GetFileName

let getImageUrl = sprintf "/images/%s"

let rnd = new Random()
let getRandomImage () =
    rnd.Next <| Array.length images
    |> Array.get images
    |> fun img -> { name = img; url = getImageUrl img }

// Web parts

let template = File.ReadAllText (__SOURCE_DIRECTORY__ + "/templates/index.html")
let index =
    fun _ ->
        let imageData = getRandomImage()
        template.Replace("[IMAGE-NAME]", imageData.name)
                .Replace("[IMAGE-URL]", imageData.url)
        |> Successful.OK
    |> warbler

let handleChoice (req:HttpRequest) =
    let frm = req.form
    let choice = frm ^^ "choice"
    let img = frm ^^ "img"
    match img, choice with
    | Choice1Of2 img, Choice1Of2 choice ->
        JsonConvert.SerializeObject { correct = false; message = "" }
        |> Successful.OK
    | _ -> RequestErrors.BAD_REQUEST "bad"

// App

let app =
    choose [
        path "/" >>= choose [
            GET >>= index
            POST >>= request handleChoice ]
        GET >>= choose [
            Files.browse staticFilesRoot
            Files.file (Files.resolvePath staticFilesRoot "404.html") ]
        Files.file (Files.resolvePath staticFilesRoot "400.html") ]
