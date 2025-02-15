﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Neo4j.Driver.Tests.TestBackend
{
    //TransientException = DriverError
    //ClientException = ClientError
    //All others = BackendError


    /*
    neo4jException
    |  ClientException
    |  |   ValueTruncationException
    |  |   ValueOverflowException
    |  |   FatalDiscoveryException
    |  |   ResultConsumedException
    |  TransientException
    |  DatabaseException
    |  ServiceUnavailableException
    |  SessionExpiredException
    |  ProtocolException
    |  SecurityException
    |  |   AuthenticationException
    */

    internal static class ExceptionManager
    {
        private static Dictionary<Type, string> TypeMap { get; set; } = new Dictionary<Type, string>()
        {
            { typeof(Neo4jException),                   "Neo4jError" },
            { typeof(ClientException),                  "ClientError" },
            { typeof(TransientException),               "DriverError" },        //Should maybe Transient error, talk to Peter or Martin
            { typeof(DatabaseException),                "DatabaseError" },
            { typeof(ServiceUnavailableException),      "ServiceUnavailableError" },
            { typeof(SessionExpiredException),          "SessionExpiredError" },
            { typeof(Driver.ProtocolException),         "ProtocolError" },
            { typeof(SecurityException),                "SecurityError" },
            { typeof(AuthenticationException),          "AuthenticationError" },
            { typeof(AuthorizationException),           "AuthorizationExpired" },
            { typeof(ValueTruncationException),         "ValueTruncationError" },
            { typeof(ValueOverflowException),           "ValueOverflowError" },
            { typeof(FatalDiscoveryException),          "FatalDiscoveryError" },
            { typeof(ResultConsumedException),          "ResultConsumedError" },
            { typeof(TransactionNestingException),      "TransactionNestingException" },
            { typeof(TokenExpiredException),			"TokenExpiredError"  },
            { typeof(ConnectionReadTimeoutException),   "ConnectionReadTimeoutError"},
            { typeof(InvalidBookmarkException),			"InvalidBookmarkError"},
            { typeof(TransactionClosedException),       "ClientError"},

            { typeof(NotSupportedException),            "NotSupportedException" },

            { typeof(ArgumentException),				"ArgumentError"}
        };


        internal static ProtocolResponse GenerateExceptionResponse(Exception ex)
        {
            string outerExceptionMessage = ex.Message;
            string exceptionMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;

            var type = TypeMap.GetValueOrDefault(ex.GetType());


            //if (ex is Neo4jException || ex is NotSupportedException)
            if (type is not null)
            {
                ProtocolException newError = ProtocolObjectFactory.CreateObject<ProtocolException>();
                newError.ExceptionObj = ex;
                string errorCode = (ex is Neo4jException) ? ((Neo4jException)ex).Code : type;
                return new ProtocolResponse("DriverError", new
                {
                    id = newError.uniqueId,
                    errorType = type,
                    msg = exceptionMessage,
                    code = errorCode
                });
            }
            else if (ex is DriverExceptionWrapper)
            {
                ProtocolException newError = ProtocolObjectFactory.CreateObject<ProtocolException>();
                return new ProtocolResponse("DriverError", new
                {
                    id = newError.uniqueId,
                    errorType = ex.InnerException.GetType().Name,
                    msg = exceptionMessage
                });
            }
            else if (ex is TestKitClientException)
            {
                return new ProtocolResponse("FrontendError", new
                {
                    msg = ex.Message
                });
            }

            Trace.WriteLine($"Exception thrown {outerExceptionMessage}\n     which contained -- {exceptionMessage}\n{ex.StackTrace}");
            return new ProtocolResponse("BackendError", new { msg = exceptionMessage } );
        }
    }
}
