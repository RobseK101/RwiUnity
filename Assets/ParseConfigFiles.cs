using System;
using System.Collections.Generic;
using System.IO;

namespace ren
{
    public enum ObjsEntryType
    {
        NON_BREAKABLE,
        BREAKABLE_SIMPLE,
        BREAKABLE_COMPLEX,
    }

    [Flags]
    public enum IdeFlags
    {
        NONE = 0,
        UNKNOWN1 = 0x1,
        DO_NOT_FADE = 0x2,
        DRAW_LAST = 0x4,
        ADDITIVE = 0x8,
        IS_SUBWAY = 0x10,
        IGNORE_LIGHTING = 0x20,
        NO_ZBUFFER_WRITE = 0x40
    }

    public struct ObjsEntry
    {
        public ObjsEntryType type;
        public int id;
        public string dffName;
        public string txdName;
        public int meshCount;
        public float drawDistance1;
        public float drawDistance2;
        public float drawDistance3;
        public IdeFlags flags;
    }

    public static class Stringtools
    {
        public static string SanitizeIDE(string line)
        {
            if (line == null)
            {
                return string.Empty;
            }

            int commentStart = line.IndexOf('#');
            string lineNocomment;
            if (commentStart == - 1)
            {
                lineNocomment = line;
            }
            else
            {
                lineNocomment = line.Substring(0, commentStart);
            }
            return lineNocomment.Trim();
        }

        public static ObjsEntry ConvertObjsLine(string line)
        {
            line = line.Replace(',', ' ');
            string[] items = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (items.Length == 6)
            {
                ObjsEntry entry = new ObjsEntry
                {
                    type = ObjsEntryType.NON_BREAKABLE
                };
                if (!Int32.TryParse(items[0], out entry.id))
                {
                    throw new Exception("Invalid item #0 (ID) in objs line \"" +  line + '\"');
                }
                entry.dffName = items[1];
                entry.txdName = items[2];
                if (!Int32.TryParse(items[3], out entry.meshCount))
                {
                    throw new Exception("Invalid item #3 (mesh count) in objs line \"" + line + '\"');
                }
                if (!float.TryParse(items[4], out entry.drawDistance1))
                {
                    throw new Exception("Invalid item #4 (draw distance 1) in objs line \"" + line + '\"');
                }
                entry.drawDistance2 = float.NaN;
                entry.drawDistance3 = float.NaN;
                uint flags;
                if (!uint.TryParse(items[5], out flags))
                {
                    throw new Exception("Invalid item #5 (flags) in objs line \"" + line + '\"');
                }
                entry.flags = (IdeFlags)flags;
                return entry;
            }
            else if (items.Length == 7)
            {
                ObjsEntry entry = new ObjsEntry
                {
                    type = ObjsEntryType.BREAKABLE_SIMPLE
                };
                if (!Int32.TryParse(items[0], out entry.id))
                {
                    throw new Exception("Invalid item #0 (ID) in objs line \"" + line + '\"');
                }
                entry.dffName = items[1];
                entry.txdName = items[2];
                if (!Int32.TryParse(items[3], out entry.meshCount))
                {
                    throw new Exception("Invalid item #3 (mesh count) in objs line \"" + line + '\"');
                }
                if (!float.TryParse(items[4], out entry.drawDistance1))
                {
                    throw new Exception("Invalid item #4 (draw distance 1) in objs line \"" + line + '\"');
                }
                if (!float.TryParse(items[5], out entry.drawDistance2))
                {
                    throw new Exception("Invalid item #5 (draw distance 2) in objs line \"" + line + '\"');
                }
                entry.drawDistance3 = float.NaN;
                uint flags;
                if (!uint.TryParse(items[6], out flags))
                {
                    throw new Exception("Invalid item #6 (flags) in objs line \"" + line + '\"');
                }
                entry.flags = (IdeFlags)flags;
                return entry;
            }
            else if (items.Length == 8)
            {
                ObjsEntry entry = new ObjsEntry
                {
                    type = ObjsEntryType.BREAKABLE_COMPLEX
                };
                if (!Int32.TryParse(items[0], out entry.id))
                {
                    throw new Exception("Invalid item #0 (ID) in objs line \"" + line + '\"');
                }
                entry.dffName = items[1];
                entry.txdName = items[2];
                if (!Int32.TryParse(items[3], out entry.meshCount))
                {
                    throw new Exception("Invalid item #3 (mesh count) in objs line \"" + line + '\"');
                }
                if (!float.TryParse(items[4], out entry.drawDistance1))
                {
                    throw new Exception("Invalid item #4 (draw distance 1) in objs line \"" + line + '\"');
                }
                if (!float.TryParse(items[5], out entry.drawDistance2))
                {
                    throw new Exception("Invalid item #5 (draw distance 2) in objs line \"" + line + '\"');
                }
                if (!float.TryParse(items[6], out entry.drawDistance3))
                {
                    throw new Exception("Invalid item #6 (draw distance 3) in objs line \"" + line + '\"');
                }
                uint flags;
                if (!uint.TryParse(items[7], out flags))
                {
                    throw new Exception("Invalid item #6 (flags) in objs line \"" + line + '\"');
                }
                entry.flags = (IdeFlags)flags;
                return entry;
            }
            else
            {
                throw new Exception("Invalid number of items (" + items.Length + ") in objs line \"" + line + '\"');
            }
        }
    }

    public class IdeFile
    {
        public IdeFile(string filename)
        {
            textfile = new StreamReader(filename);
        }

        private enum ObjsFSMState
        {
            FIND_BLOCK_START,
            HANDLE_LINE,
            EXIT
        }

        public ObjsEntry[] GetObjs()
        {
            ObjsFSMState state = ObjsFSMState.FIND_BLOCK_START;

            List<ObjsEntry> result = new List<ObjsEntry>();
            string currentLine = textfile.ReadLine();

            while (currentLine != null)
            {
                string lineSanitized = Stringtools.SanitizeIDE(currentLine);

                switch (state)
                {
                    case ObjsFSMState.FIND_BLOCK_START:
                        {
                            if (lineSanitized == "objs")
                            {
                                currentLine = textfile.ReadLine();
                                state = ObjsFSMState.HANDLE_LINE;
                                continue;
                            }
                            else
                            {
                                currentLine = textfile.ReadLine();
                                continue;
                            }
                        }
                    case ObjsFSMState.HANDLE_LINE:
                        {
                            if (lineSanitized == "end")
                            {
                                currentLine = textfile.ReadLine();
                                state = ObjsFSMState.FIND_BLOCK_START;
                                continue;
                            }

                            if (lineSanitized == string.Empty)
                            {
                                currentLine = textfile.ReadLine();
                                continue;
                            }

                            result.Add(Stringtools.ConvertObjsLine(lineSanitized));
                            currentLine = textfile.ReadLine();
                            continue;
                        }
                    case ObjsFSMState.EXIT:
                        {
                            currentLine = null;
                            break;
                        }
                }
            }

            return result.ToArray();
        }

        private StreamReader textfile;
    }
}