﻿using Docdown.Util;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public interface IWorkspaceItem
    {
        string RelativeName { get; }
        string Name { get; }
        string FullName { get; }
        FileSystemInfo FileInfo { get; set; }
        WorkspaceItemType Type { get; set; }
        List<IWorkspaceItem> Children { get; }
        IWorkspace Workspace { get; set; }
        IWorkspaceItem Parent { get; set; }
        IWorkspaceItem TopParent { get; }
        ConverterType FromType { get; }
        ConverterType ToType { get; set; }
        //bool IsHidden { get; set; }
        bool IsDirectory { get; }
        bool IsFile { get; }

        Task<byte[]> Read();
        Task Save(string text);
        Task<string> Convert(CancelToken cancelToken);
        Task Delete();
        Task Rename(string newName);
        Task Update();
        Task<IWorkspaceItem> CreateNewFile(string name, string autoExtension = null, byte[] content = null);
        Task<IWorkspaceItem> CreateNewDirectory(string name);
        Task<IWorkspaceItem> CopyExistingItem(string path);
        Task<IWorkspaceItem> CopyExistingFolder(string path);
    }
}