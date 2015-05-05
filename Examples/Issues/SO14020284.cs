﻿using Xunit;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Examples.Issues
{
    
    public class SO14020284
    {
        [Fact]
        public void Execute()
        {
            var model = RuntimeTypeModel.Create();
            model.AutoCompile = false;
            Execute(model, "Runtime");
            model.CompileInPlace();
            Execute(model, "CompileInPlace");
            //Execute(model.Compile(), "Compile");

            model.CompileIntoTestFolder("SO14020284", "SO14020284.dll");
            PEVerify.AssertValid("SO14020284.dll");

        }
        public void Execute(TypeModel model, string caption)
        {
            try
            {
                var ms = new MemoryStream();
                model.Serialize(ms, new EncapsulatedOuter { X = 123, Inner = new EncapsulatedInner { Y = 456 } });
                ms.Position = 0;
                var obj = (InheritedChild)model.Deserialize(ms, null, typeof(InheritedBase));
                Assert.Equal(123, obj.X); //, caption);
                Assert.Equal(456, obj.Y); //, caption);
            }
            catch (Exception ex)
            {
                Assert.Equal("##fail##", caption + ":" + ex.Message);
            }
        }
        [ProtoContract]
        public class EncapsulatedOuter
        {
            [ProtoMember(10)]
            public EncapsulatedInner Inner { get; set; }

            [ProtoMember(1)]
            public int X { get; set; }
        }
        [ProtoContract]
        public class EncapsulatedInner
        {
            [ProtoMember(1)]
            public int Y { get; set; }
        }
        [ProtoContract]
        [ProtoInclude(10, typeof(InheritedChild))]
        public class InheritedBase
        {
            [ProtoMember(1)]
            public int X { get; set; }
        }
        [ProtoContract]
        public class InheritedChild : InheritedBase
        {
            [ProtoMember(1)]
            public int Y { get; set; }
        }
    }
}
