﻿using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

namespace EA4S.Db.Loader
{
    public class StageParser : DataParser<StageData, StageTable>
    {
        override protected StageData CreateData(Dictionary<string, object> dict, Database db)
        {
            var data = new StageData();

            data.Id = ToString(dict["Id"]);
            data.Title_En = ToString(dict["Title_En"]);
            data.Title_Ar = ToString(dict["Title_Ar"]);
            data.Description = ToString(dict["Description"]);

            return data;
        }
    }
}
