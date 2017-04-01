#if INTERACTIVE
#I "../Packages/Fsharp.Data/lib/net40"
#I "../Packages/Newtonsoft.Json/lib/net45"
#endif

#r "System.Net.Http"
#r "Newtonsoft.Json"
#r "Fsharp.Data"
#load "Types.fs"
#load "Statistics.fs"

open System.Net
open System.Net.Http
open System.Net.Http.Headers
open Newtonsoft.Json
open FSharp.Data
//open Types
open Statistics

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        log.Info(sprintf 
            "F# HTTP trigger function processed a request.")

        let name =
            req.GetQueryNameValuePairs()
            |> Seq.tryFind (fun q -> q.Key.ToLowerInvariant() = "name")

        let response =
            match name with
            | Some x ->
                let statistics = getNameStatistics x.Value
                match statistics with
                | Some y -> 
                    let json = JsonConvert.SerializeObject(y)
                    let jsonResponse = sprintf "%s" json
                    req.CreateResponse(HttpStatusCode.OK, jsonResponse, "text/plain")
                | None -> req.CreateResponse(HttpStatusCode.BadRequest, "We haven't found the name")
            | None ->
                req.CreateResponse(HttpStatusCode.BadRequest, "Specify a Name value")

        return response

    } |> Async.RunSynchronously
