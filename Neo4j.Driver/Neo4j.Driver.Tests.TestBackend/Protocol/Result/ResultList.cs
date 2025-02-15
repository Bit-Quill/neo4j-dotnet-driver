﻿// Copyright (c) 2002-2022 "Neo4j,"
// Neo4j Sweden AB [http://neo4j.com]
// 
// This file is part of Neo4j.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Neo4j.Driver.Tests.TestBackend
{
    internal class ResultList : IProtocolObject
    {
        public ResultListType data { get; set; } = new ResultListType();

        [JsonIgnore]
        public List<IRecord> Records { get; set; }

        public class ResultListType
        {
            public string resultId { get; set; }
        }

        public override async Task Process()
        {
            var result = (Result)ObjManager.GetObject(data.resultId);
            Records = await result.ToListAsync();
        }

        public override string Respond()
        {
            if (Records == null) 
                return new ProtocolResponse("NullRecord", (object) null).Encode();

            var mappedList = Records
                .Select(x => new
                {
                    values = x.Values
                        .Select(y => NativeToCypher.Convert(y.Value))
                        .ToList()
                })
                .ToList();

            return new ProtocolResponse("RecordList", new { records = mappedList }).Encode();
        }
    }
}