namespace Types

    open FSharp.Data
    open System.Runtime.Serialization

    type NameData = CsvProvider<"Order,Name,Frequency,AverageAge", HasHeaders = true, 
                                                Schema = "Order(int),Name,Frequency(int), AverageAge(float)">

    type NameStatistic = {Frequency: int}

    [<DataContract>]
    type Result = {
        [<field: DataMember(Name="Gender")>]
        Gender: string
        [<field: DataMember(Name="Frequency")>]
        Frequency: int
        [<field: DataMember(Name="Percentage")>]
        Percentage:float
    }