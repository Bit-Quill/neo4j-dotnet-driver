﻿using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Neo4j.Driver;
using System.Collections.Generic;
using System.Diagnostics;


namespace Neo4j.Driver.Tests.TestBackend
{
	internal class Result : IProtocolObject
	{
		public ResultType data { get; set; } = new ResultType();

        public class ResultType
        {
            public string id { get; set; }
        }

        public override async Task Process()
        {
            //Currently does nothing
            await Task.CompletedTask;
        }

        public override string Respond()
        {
            return new ProtocolResponse("Result", uniqueId).Encode();
        }

		public async virtual Task<IRecord> GetNextRecord()
		{
			return await Task.FromResult<IRecord>(null);
		}

		public async virtual Task<IResultSummary> ConsumeResults()
		{
			return await Task.FromResult<IResultSummary>(null);
		}
	}

	internal class TransactionResult : Result
	{
		[JsonIgnore]
		private IResultCursor ResultCursor { get; set; }


		public async override Task<IRecord> GetNextRecord()
		{
			if(await ResultCursor.FetchAsync())
			{
				return await Task.FromResult<IRecord>(ResultCursor.Current);
			}

			return await Task.FromResult<IRecord>(null);
		}

		public async Task PopulateRecords(IResultCursor cursor)
		{
			ResultCursor = cursor;
			await Task.CompletedTask;
		}

		public async override Task<IResultSummary> ConsumeResults()
		{
			return await ResultCursor.ConsumeAsync().ConfigureAwait(false);
		}
	}


	internal class SessionResult : Result
	{
		[JsonIgnore]
		public IResultCursor Results { private get; set; }

		public async override Task<IRecord> GetNextRecord()
		{
			if (await Results.FetchAsync().ConfigureAwait(false))
			{
				return await Task.FromResult<IRecord>(Results.Current);
			}

			return await Task.FromResult<IRecord>(null);
		}

		public async override Task<IResultSummary> ConsumeResults()
		{
			return await Results.ConsumeAsync().ConfigureAwait(false);
		}
	}
}
