﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OZD.SmartExam.Library.DataModels
{
    public class HistoryDataModel
    {
        public string Title { get; set; }

        public IEnumerable<ChartSeriesItem> Data { get; set; }
    }
}
