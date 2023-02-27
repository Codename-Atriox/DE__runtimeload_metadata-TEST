using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Runtime.InteropServices;

namespace quick_manifest_test
{
    internal class Program
    {
        static int ReadInt(FileStream fs)
        {
            byte[] bytes = new byte[4];
            fs.Read(bytes, 0, 4);
            return BitConverter.ToInt32(bytes, 0);
        }
        static short ReadShort(FileStream fs)
        {
            byte[] bytes = new byte[2];
            fs.Read(bytes, 0, 2);
            return BitConverter.ToInt16(bytes, 0);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            string metadata_path = "C:\\Users\\Joe bingle\\Downloads\\rlm safe";


            runtime_manifest runtime_meterdata = new();
            using (FileStream fs = new(metadata_path, FileMode.Open, FileAccess.Read))
            {
                runtime_meterdata.TagCount = ReadInt(fs);
                runtime_meterdata.tags = new runtimeload_metadata_tag[runtime_meterdata.TagCount];
                for (int i = 0; i < runtime_meterdata.TagCount; i++)
                {
                    // read a single tag instance structure
                    runtimeload_metadata_tag curr_tag_inst = new();
                    curr_tag_inst.CachedTagID = ReadInt(fs);
                    // process the array of linked_tags
                    curr_tag_inst.linked_tag_count = ReadInt(fs);
                    curr_tag_inst.linked_Tags = new linked_tag[curr_tag_inst.linked_tag_count];
                    for (int link_index = 0; link_index < curr_tag_inst.linked_tag_count; link_index++)
                    {
                        linked_tag curr_link = new();
                        curr_link.ZonesetStringID = ReadInt(fs);
                        curr_link.Unk_0x04 = ReadInt(fs);
                        curr_link.Unk_0x08 = ReadShort(fs);
                        // DEBUG // DEBUG // DEBUG //
                        if (curr_link.Unk_0x08 != 0 || curr_link.Unk_0x04 != -1)
                        {
                            // put a breakpoint here if you want to test for unknown values
                        }

                        // process the dependent tag array
                        curr_link.DependentTagCount = ReadInt(fs);
                        curr_link.dependent_tags = new int[curr_link.DependentTagCount];
                        for (int d_tag_index = 0; d_tag_index < curr_link.DependentTagCount; d_tag_index++)
                            curr_link.dependent_tags[d_tag_index] = ReadInt(fs);

                        // process the double tagid array
                        curr_link.amount_of_sublinks = ReadInt(fs);
                        curr_link.sublinks = new tag_link[curr_link.amount_of_sublinks];
                        for (int tagiddouble_index = 0; tagiddouble_index < curr_link.amount_of_sublinks; tagiddouble_index++)
                        {
                            curr_link.sublinks[tagiddouble_index].tagid = ReadInt(fs);
                            curr_link.sublinks[tagiddouble_index].parent_tagid = ReadInt(fs);
                        }


                        curr_tag_inst.linked_Tags[link_index] = curr_link;
                    }


                    runtime_meterdata.tags[i] = curr_tag_inst;
                }
            }
            // just pop a breakpoint at the end here, so you can view through the list

        }
        public struct runtime_manifest
        {
            public int TagCount;
            public runtimeload_metadata_tag[] tags;
        }
        public struct runtimeload_metadata_tag
        {
            public int CachedTagID;         // tag id referencing a tag inside this module, might also be allowed to index tags not in this module
            public int linked_tag_count;    // the total amount to tags linked to this ID     
            public linked_tag[] linked_Tags;       // an array of all the tags that are probably related to this one
        }
        public struct linked_tag
        {
            public int ZonesetStringID;    // name of the zoneset that this tag will belong to
            public int Unk_0x04;            // only seen as -1 so far
            public short Unk_0x08;             // only seen as 0 so far
            public int DependentTagCount;  // this tells us how many TagIDs to read for the tags that this tag depends on
            public int[] dependent_tags;   // a list of the tagID's that this tagid relies on, requiring these to load if this tag loads
            public int amount_of_sublinks;    // this is either 0 or 1, it indicates whether to read zoneset structs (or rather if they are present)
            public tag_link[] sublinks; // length = zoneset_count
        }
        public struct tag_link
        {
            public int tagid;
            public int parent_tagid;
        }
    }
}