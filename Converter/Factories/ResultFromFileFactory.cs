using Converter.Queries.GetResultFromFile.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Converter.Factories
{
    public class ResultFromFileFactory
    {
        public IResultFromFile CreateResultFromFile(string type, string[] resultFromFile)
        {
            string[] columns = resultFromFile[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (type)
            {
                case "Split-Post":
                    return CreateSplitPostResut(columns);
                case "Single-Post":
                    return CreateSinglePostResut(columns);
                default:
                    return new SplitPostResult();
            }
        }

        SplitPostResult CreateSplitPostResut(string[] columns)
        {
            var result = new SplitPostResult { H = columns[0] };

            if (columns.Length > 2)
            {
                result.Permittivity = columns[1];
                result.DielectricLossTangent = columns[2];  
                if (columns.Length > 3)
                    result.Resistivity = columns[3]; 
            }
            else
                result.SheetRessistance = columns[0];

            return result;
        }

        SinglePostResult CreateSinglePostResut(string[] columns)
        {
            var result = new SinglePostResult { H = columns[0] };

            if (columns.Length > 1)
               result.Resistivity = columns[1];
            else
                result.SheetRessistance = columns[0];

            return result;
        }
    }
}
