﻿/* Copyright 2012 Aaron Dierking, TJ Tunnell, Jordan Mueller, Alex Reed
 * 
 * This file is part of ExtryzeDLL.
 * 
 * Extryze is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Extryze is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with ExtryzeDLL.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtryzeDLL.Blam.ThirdGen.Structures;
using ExtryzeDLL.Blam.Util;
using ExtryzeDLL.Flexibility;
using ExtryzeDLL.IO;
using ExtryzeDLL.Util;

namespace ExtryzeDLL.Blam.ThirdGen
{
    /// <summary>
    /// A third-generation Blam (map) cache file.
    /// </summary>
    public class ThirdGenCacheFile : ICacheFile
    {
        private ThirdGenHeader _header;
        private ThirdGenTagTable _tags;
        private ThirdGenStringIDSource _stringIds;
        private ThirdGenFileNameSource _fileNames;
        private ThirdGenLanguageGlobals _languageInfo;
        private ThirdGenScenarioMeta _scenario;
        private List<ILanguage> _languages = new List<ILanguage>();

        public ThirdGenCacheFile(IReader reader, BuildInformation buildInfo, string buildString)
        {
            Load(reader, buildInfo, buildString);
        }

        public ICacheFileInfo Info
        {
            get { return _header; }
        }

        public PointerConverter MetaPointerConverter
        {
            get { return _header.MetaPointerConverter; }
        }

        public PointerConverter LocalePointerConverter
        {
            get { return _header.LocalePointerConverter; }
        }

        public IFileNameSource FileNames
        {
            get { return _fileNames; }
        }

        public IStringIDSource StringIDs
        {
            get { return _stringIds; }
        }

        public IList<ILanguage> Languages
        {
            get { return _languages.AsReadOnly(); }
        }

        public IList<ITagClass> TagClasses
        {
            get { return _tags.Classes; }
        }

        public IList<ITag> Tags
        {
            get { return _tags.Tags; }
        }

        public IScenario Scenario
        {
            get { return _scenario; }
        }

        private void Load(IReader reader, BuildInformation buildInfo, string buildString)
        {
            _header = LoadHeader(reader, buildInfo, buildString);
            _fileNames = LoadFileNames(reader, buildInfo);
            _stringIds = LoadStringIDs(reader, buildInfo);
            _tags = LoadTags(reader, buildInfo);
            _languageInfo = LoadLanguageGlobals(reader, buildInfo);
            _scenario = LoadScenario(reader, buildInfo);

            BuildLanguageList();
        }

        private ThirdGenHeader LoadHeader(IReader reader, BuildInformation buildInfo, string buildString)
        {
            reader.SeekTo(0);
            StructureValueCollection values = StructureReader.ReadStructure(reader, buildInfo.GetLayout("header"));
            return new ThirdGenHeader(values, buildInfo, buildString);
        }

        private ThirdGenTagTable LoadTags(IReader reader, BuildInformation buildInfo)
        {
            reader.SeekTo(_header.IndexHeaderLocation.AsOffset());
            StructureValueCollection values = StructureReader.ReadStructure(reader, buildInfo.GetLayout("index header"));
            return new ThirdGenTagTable(reader, values, _header.MetaPointerConverter, buildInfo);
        }

        private ThirdGenFileNameSource LoadFileNames(IReader reader, BuildInformation buildInfo)
        {
            return new ThirdGenFileNameSource(reader, _header.FileNameCount, _header.FileNameTableSize, _header.FileNameIndexTableLocation, _header.FileNameDataLocation, buildInfo);
        }

        private ThirdGenStringIDSource LoadStringIDs(IReader reader, BuildInformation buildInfo)
        {
            return new ThirdGenStringIDSource(reader, _header.StringIDCount, _header.StringIDTableSize, _header.StringIDIndexTableLocation, _header.StringIDDataLocation, buildInfo);
        }

        private ThirdGenLanguageGlobals LoadLanguageGlobals(IReader reader, BuildInformation buildInfo)
        {
            // Check for a PATG tag, and if one isn't found, then use MATG
            ITag tag = null;
            StructureLayout layout = null;
            if (buildInfo.HasLayout("patg"))
            {
                tag = FindTagByClass(PatgMagic);
                layout = buildInfo.GetLayout("patg");
            }
            if (tag == null)
            {
                tag = FindTagByClass(MatgMagic);
                layout = buildInfo.GetLayout("matg");
            }
            if (tag == null || layout == null)
                throw new InvalidOperationException("The cache file is missing locale information.");

            reader.SeekTo(tag.MetaLocation.AsOffset());
            StructureValueCollection values = StructureReader.ReadStructure(reader, layout);
            return new ThirdGenLanguageGlobals(values, _header.LocalePointerConverter, buildInfo);
        }

        private void BuildLanguageList()
        {
            // hax hax hax
            foreach (ThirdGenLanguage language in _languageInfo.Languages)
                _languages.Add(language);
        }

        private ThirdGenScenarioMeta LoadScenario(IReader reader, BuildInformation buildInfo)
        {
            if (!buildInfo.HasLayout("scnr"))
                return null;
            ITag scnr = FindTagByClass(ScnrMagic);
            if (scnr == null)
                throw new InvalidOperationException("The cache file is missing a scnr tag.");

            reader.SeekTo(scnr.MetaLocation.AsOffset());
            StructureValueCollection values = StructureReader.ReadStructure(reader, buildInfo.GetLayout("scnr"));
            return new ThirdGenScenarioMeta(values, reader, _header.MetaPointerConverter, _stringIds, buildInfo);
        }

        private ITag FindTagByClass(int classMagic)
        {
            foreach (ITag tag in _tags.Tags)
            {
                if (tag != null && tag.Class != null && tag.Class.Magic == classMagic)
                    return tag;
            }
            return null;
        }

        private static int MatgMagic = CharConstant.FromString("matg");
        private static int PatgMagic = CharConstant.FromString("patg");
        private static int ScnrMagic = CharConstant.FromString("scnr");
    }
}
