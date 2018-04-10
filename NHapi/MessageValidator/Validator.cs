/*
 * NHapi does not throw any errors on hl7 v2 structure failures but it constructs the object.
 * This code helps to find out structural errors by using ExtraComponent property on IType.
 * Version 1.0.0.0
 * Auther: Shivkumar Haldikar
 * Date: 04/10/2018
 */

namespace NHapi.MessageValidator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NHapi.Base.Parser;
    using NHapi.Base.Model;
    using NHapi.Model.V26.Segment;
    using System.Reflection;

    public class NHapiValidator
    {
        private List<string> errorCollection = new List<string>();
        private string currentSegment = string.Empty;
        private int fieldNumber = 0;
        private List<CSegment> segments = new List<CSegment>();

        public List<string> ErrorCollection
        {
            get { return errorCollection; }
        }

        public bool Validate(string hl7Text, string version)
        {
            IMessage message = new PipeParser().Parse(hl7Text, version);
            AbstractGroup grp = message as AbstractGroup;
            if (grp != null)
            {
                foreach (string name in grp.Names)
                {
                    LoopOnSegments(grp.GetAll(name));
                }
            }
            return errorCollection.Count > 0;
        }

        //loop through each segment
        private void LoopOnSegments(IStructure[] structures)
        {
            //Structure could be segment or a group
            foreach (IStructure structure in structures)
            {
                AbstractSegment segment = structure as AbstractSegment;
                if (segment == null)
                {
                    AbstractGroup grp = structure as AbstractGroup;
                    foreach (string name in grp.Names)
                        LoopOnSegments(grp.GetAll(name));
                }
                else
                {
                    currentSegment = segment.GetStructureName();
                    IEnumerable<CSegment> csegs = segments.Where(cseg => cseg.Name.Equals(currentSegment));
                    if (csegs.Count() > 0)
                        csegs.First().FieldNumber += 1;
                    else
                        segments.Add(new CSegment() { Name = currentSegment, FieldNumber = 1 });

                    for (int i = 1; i <= segment.NumFields(); i++)
                    {
                        fieldNumber = i;
                        LoopOnFields(segment.GetField(i));
                    }
                }
            }
        }

        //loop throgh each field
        private void LoopOnFields(IType[] types)
        {
            foreach (IType type in types)
            {
                //Field could be composite or primitive
                IComposite compositeItem = type as IComposite;
                if (compositeItem != null)
                    LoopOnFields((IType[])compositeItem.Components);
               
                //Here we can find structural issues.
                if (type.ExtraComponents.numComponents() > 0)
                {
                    CSegment _cseg = segments.Where(cseg => cseg.Name.Equals(currentSegment)).First<CSegment>();
                    errorCollection.Add(string.Format("{0}^{1}^{2}^102&Data type error&HL7nnnn", currentSegment, _cseg.FieldNumber, fieldNumber));
                }
            }
        }

    }
}

class CSegment
{
    public string Name
    {
        get;
        set;
    }

    public int FieldNumber
    {
        get;
        set;
    }
}
