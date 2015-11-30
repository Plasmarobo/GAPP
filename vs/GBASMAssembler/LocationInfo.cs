using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBASMAssembler
{
    public enum Locations
    {
        NONE = -1,
        B = 0,
        C,
        D,
        E,
        H,
        L,
        HL, //Hack to allow HL in linear range
        A,
        F,
        AF,
        BC,
        DE,
        SP,
        PC,
        Z,
        NC,
        NZ,
        MEM, //Memory address 16b
        IMM, //8b
        WIDE_IMM, //16b
        OFFSET, //Memory address given by FF00 + n (8b)
        WIDE_OFFSET, //Memory address given by FF00+n (16b)
        //LABEL,
        STACK,
        NUM_LOCATIONS
    };

    public class LocationInfo
    {
        public Locations loc;
        public bool isMem;
        public bool isReg;
        public bool isFlag;
        public bool isOffset;
        public bool isWide;
        public Int16 val;

        public LocationInfo()
        {
            loc = Locations.NONE;
            isMem = false;
            isReg = false;
            isFlag = false;
            isOffset = false;
            isWide = false;
            val = 0;
        }
    }
}
