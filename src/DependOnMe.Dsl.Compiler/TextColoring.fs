module TextColoring

type TermColor = { Classifier: string; StartPos: int; Length: int; }

let createColor classifier startPos length =
    { 
        Classifier = classifier;
        StartPos = startPos; 
        Length = length; 
    }