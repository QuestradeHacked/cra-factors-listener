using System;
using System.Collections.Generic;
using CRA.FactorsListener.Cdc.Models.Enums;

namespace CRA.FactorsListener.Cdc.Utils
{
    public static class OperationTypeUtils
    {
        private static readonly IReadOnlyDictionary<string, OperationType> OperationTypes = new Dictionary<string, OperationType>
            {
                ["c"] = OperationType.Create,
                ["u"] = OperationType.Update,
                ["r"] = OperationType.Read,
                ["d"] = OperationType.Delete,
            };

        public static OperationType Create(string operation)
        {
            var isValid = OperationTypes.TryGetValue(operation, out var operationType);

            if (!isValid)
            {
                throw new InvalidOperationException("Unsupported operation type");
            }

            return operationType;
        }
    }
}