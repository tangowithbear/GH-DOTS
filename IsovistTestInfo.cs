using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace ISM {
    public class IsovistTestInfo : GH_AssemblyInfo {
        public override string Name => "ISM";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("88D89299-DE92-4BB6-AB8E-6CD4E17A0125");

        //Return a string identifying you or your company.
        public override string AuthorName => "Marina";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}