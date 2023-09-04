using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SctvMulticastGenerator.Model
{
    internal class SctvModel
    {
        [Description("序号")]
        internal int Index { get; set; }
        internal string Name { get; set; }
        internal string MulticastAddress { get; set; }
        internal int PalyBackDays { get; set; }
        internal int Id { get; set; }
        internal string Format { get; set; }
        internal string PalyBackAddress { get; set; }
    }
}
