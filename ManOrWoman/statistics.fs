module Statistics
    open Types
    open FSharp.Data

    let getGenderStatistics (fileName:string) (name:string) =
        let names = NameData.Load(fileName)

        let nameData =
            names.Rows
            |> Seq.filter(fun r -> r.Name = name.ToUpperInvariant() )
            |> Seq.tryHead
            
        match nameData with
        | None -> None
        | Some x -> Some {NameStatistic.Frequency = x.Frequency}

    let getNameStatistics (name: string) (folder:string) =
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
