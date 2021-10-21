using System.Collections.Generic;
using System.Linq;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Buffers.Arrays;

namespace CafeLib.BsvSharp.Builders
{
    public class DataScriptBuilder : ScriptBuilder
    {
        private readonly List<ByteArrayBuffer> _dataCache;

        /// <summary>
        /// DataScriptBuilder default constructor.
        /// </summary>
        public DataScriptBuilder()
        {
            _dataCache = new List<ByteArrayBuffer>();
        }

        /// <summary>
        /// DataScriptBuilder constructor.
        /// </summary>
        /// <param name="data">data</param>
        public DataScriptBuilder(byte[] data)
            : this()
        {
            _dataCache.Add(data);
        }

        /// <summary>
        /// Add bytes to data builder.
        /// </summary>
        /// <param name="data"></param>
        public override ScriptBuilder Add(byte[] data)
        {
            _dataCache.Add(data);
            return this;
        }

        /// <summary>
        /// Clear the builder.
        /// </summary>
        public override ScriptBuilder Clear()
        {
            _dataCache.Clear();
            return base.Clear();
        }

        /// <summary>
        /// Convert builder to script.
        /// </summary>
        /// <returns></returns>
        public override Script ToScript()
        {
            base.Clear();
            Add(Opcode.OP_FALSE);
            Add(Opcode.OP_RETURN);

            if (_dataCache == null || !_dataCache.Any())
            {
                return base.ToScript();
            }

            _dataCache.ForEach(x =>
            {
                if (x is null || !x.Any()) return;
                Push(x.ToArray());
            });

            return base.ToScript();
        }
    }
}
