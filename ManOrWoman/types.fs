namespace Types
    open FSharp.Data
    
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