using System;
using System.Text;

namespace HexagonalLib
{
    public class HexagonalException : Exception
    {
        private class MessageBuilder
        {
            private readonly StringBuilder _Message = new();

            public MessageBuilder Append(string message)
            {
                _Message.AppendLine(message);
                return this;
            }

            public MessageBuilder Append(HexagonalGrid grid)
            {
                Append(nameof(grid.Type), grid.Type);
                Append(nameof(grid.InscribedRadius), grid.InscribedRadius);
                Append(nameof(grid.DescribedRadius), grid.DescribedRadius);
                return this;
            }

            public MessageBuilder Append(params (string, object)[] fields)
            {
                foreach (var (paramName, paramValue) in fields)
                    Append(paramName, paramValue);

                return this;
            }

            private void Append(string paramName, object paramValue) => _Message.Append($"{paramName}={paramValue}; ");

            public override string ToString() => _Message.ToString();
        }

        public HexagonalException(string message)
            : base(message)
        {
        }

        public HexagonalException(string message, params (string, object)[] fields)
            : base(CreateBuilder(message).Append(fields).ToString())
        {
        }

        public HexagonalException(string message, HexagonalGrid grid)
            : base(CreateBuilder(message).Append(grid).ToString())
        {
        }

        public HexagonalException(string message, HexagonalGrid grid, params (string, object)[] fields)
            : base(CreateBuilder(message).Append(grid).Append(fields).ToString())
        {
        }

        private static MessageBuilder CreateBuilder(string message) => new MessageBuilder().Append(message);
    }
}