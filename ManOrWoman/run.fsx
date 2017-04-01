#if INTERACTIVE
#I "../Packages/Fsharp.Data/lib/net40"
#I "../Packages/Newtonsoft.Json/lib/net45"
#endif

#r "System.Net.Http"
#r "Fsharp.Data"
#r "System.Runtime.Serialization"

#load "Types.fs"
#load "Statistics.fs"

open System.Net
open System.Net.Http
open System.Net.Http.Headers
open Statistics

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        log.Info(sprintf 
            "F# HTTP trigger function processed a request.")

        let name =
            req.GetQueryNameValuePairs()
            |> Seq.tryFind (fun q -> q.Key.ToLowerInvariant() = "name")

        #if INTERACTIVE
        let folder = __SOURCE_DIRECTORY__ + "/data/spain/"
        #else
        let folder = Environment.ExpandEnvironmentVariables(@"%HOME%\data\spain\")
        #endif

        let response =
            match name with
            | Some x ->
                let statistics = getNameStatistics x.Value folder
                match statistics with
                | Some y -> 
                    let json = JsonConvert.SerializeObject(y)
                    let jsonResponse = sprintf "%s" json
                    req.CreateResponse(HttpStatusCode.OK, jsonResponse)
                | None -> req.CreateResponse(HttpStatusCode.BadRequest, "We haven't found the name")
            | None ->
                req.CreateResponse(HttpStatusCode.BadRequest, "Specify a Name value")

        return response

    } |> Async.RunSynchronously
