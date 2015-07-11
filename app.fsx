module App

#r "packages/Suave/lib/net40/Suave.dll"

open Suave
open Suave.Http
open Suave.Http.Applicatives
open System
open System.IO

let imagePath = __SOURCE_DIRECTORY__ + "/static/images/"

let images =
    Directory.GetFiles imagePath
    |> Array.map Path.GetFileName

let getPathOrNone img =
    let path = imagePath + img
    if File.Exists path then
        Some path
    else
        None

let rnd = new Random()
let getRandomImage () =
    rnd.Next <| Array.length images
    |> Array.get images
    |> sprintf "/images/%s"

let serveImage img =
    match getPathOrNone img with
    | Some path ->
        match Writers.defaultMimeTypesMap (Path.GetExtension path) with
        | Some mimetype -> Writers.setMimeType mimetype.name
        | None -> succeed
        >>= Files.sendFile path false
    | None -> RequestErrors.NOT_FOUND "Could not find image"

let template = File.ReadAllText (__SOURCE_DIRECTORY__ + "/static/index.html")
let index =
    (fun _ ->
        template.Replace("[IMAGE-URL]", getRandomImage())
        |> Successful.OK )
    |> warbler

let app =
    choose [
        path "/" >>= index
        pathScan "/images/%s" serveImage ]
