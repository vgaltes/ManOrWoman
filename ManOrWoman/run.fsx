#if INTERACTIVE
#I "../Packages/Fsharp.Data/lib/net40"
#I "../Packages/Newtonsoft.Json/lib/net45"
#endif

#r "System.Net.Http"
#r "Newtonsoft.Json"
#r "Fsharp.Data"

open System.Net
open System.Net.Http
open System.Net.Http.Headers
open Newtonsoft.Json
open FSharp.Data
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters.Binary
open System.Runtime.Serialization.Json
open System.Xml
open System.Xml.Serialization

type Named = {
    name: string
}

type NameData = CsvProvider<"Order,Name,Frequency,AverageAge", HasHeaders = true, 
                                            Schema = "Order(int),Name,Frequency(int), AverageAge(float)">

type NameStatistic = {Frequency: int}

type Result = {
    Gender: string
    Frequency: int
    Percentage:float
}



let getGenderStatistics (fileName:string) (name:string) =
    let names = NameData.Load(fileName)

    let nameData =
        names.Rows
        |> Seq.filter(fun r -> r.Name = name.ToUpperInvariant() )
        |> Seq.tryHead
        
    match nameData with
    | None -> None
    | Some x -> Some {NameStatistic.Frequency = x.Frequency}

let getNameStatistics (name: string) (log:TraceWriter) =
    #if INTERACTIVE
    let folder = __SOURCE_DIRECTORY__ + "/data/spain/"
    #else
    let folder = Environment.ExpandEnvironmentVariables(@"%HOME%\data\spain\")
    #endif

    log.Info(sprintf "The data folder is %s" folder)
    let statistics =
        [|folder + "men.csv"; folder + "women.csv"|]
        |> Array.map(fun x -> getGenderStatistics x name)

    let calculatePercentage (x:int) (y:int) = 
        float x * 100.0 / (float x + float y)

    match statistics with
    | [|Some m;Some w|] -> 
        match (m.Frequency > w.Frequency) with
        | true -> Some {Gender = "Man"; Frequency = m.Frequency; Percentage = calculatePercentage m.Frequency w.Frequency}
        | false -> Some {Gender = "Woman"; Frequency = w.Frequency; Percentage = calculatePercentage w.Frequency m.Frequency}
    | [|Some m;None|] -> 
        Some {Gender = "Man"; Frequency = m.Frequency; Percentage = 100.0} 
    | [|None;Some w|] -> 
        Some {Gender = "Woman"; Frequency = w.Frequency; Percentage = 100.0} 
    | _ -> None

let toString = System.Text.Encoding.ASCII.GetString
let serializeJson<'a> (x : 'a) = 
    let jsonSerializer = new DataContractJsonSerializer(typedefof<'a>)

    use stream = new MemoryStream()
    jsonSerializer.WriteObject(stream, x)
    toString <| stream.ToArray()

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        log.Info(sprintf 
            "F# HTTP trigger function processed a request.")
        

        //Set name to query string
        let name =
            req.GetQueryNameValuePairs()
            |> Seq.tryFind (fun q -> q.Key.ToLowerInvariant() = "name")

        let response =
            match name with
            | Some x ->
                let statistics = getNameStatistics x.Value log
                match statistics with
                | Some y -> 
                    //let json = JsonConvert.SerializeObject(y)
                    let json = serializeJson y
                    let jsonResponse = sprintf "%s" json
                    req.CreateResponse(HttpStatusCode.OK, jsonResponse)
                | None -> req.CreateResponse(HttpStatusCode.BadRequest, "We haven't found the name")
            | None ->
                req.CreateResponse(HttpStatusCode.BadRequest, "Specify a Name value")

        return response

    } |> Async.RunSynchronously
