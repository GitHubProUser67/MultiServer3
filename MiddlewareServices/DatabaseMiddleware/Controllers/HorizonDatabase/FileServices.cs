using DatabaseMiddleware.SQLiteEngine;
using Horizon.LIBRARY.Database.Models;
using Horizon.LIBRARY.Database.Entities;
using FileAttributes = Horizon.LIBRARY.Database.Entities.FileAttributes;
using System.Data.Entity;

namespace DatabaseMiddleware.Controllers.HorizonDatabase
{
    public class FileServices
    {
        public MiddlewareSQLiteContext? db { get; set; }

        public FileServices()
        {
            db = SQLiteConnector.GetConnectionsByDatabaseName("medius_database")?.FirstOrDefault();
        }

        #region CreateFile
        public async Task<dynamic?> createFile(FileDTO fileReq)
        {
            Files? existingFile = db?.Files?.Where(af => af.FileName == fileReq.FileName && af.OwnerID == fileReq.OwnerID && af.AppId == fileReq.AppId).FirstOrDefault();
            FileAttributes? existingFileAttributes = db?.FileAttributes?.Where(af => af.AppId == fileReq.AppId && af.Description == fileReq.fileAttributesDTO.Description).FirstOrDefault();

            if (existingFile != null || existingFileAttributes != null)
                return null;

            Files newFile = new()
            {
                AppId = fileReq.AppId,
                FileName = fileReq.FileName,
                ServerChecksum = fileReq.ServerChecksum,
                FileID = fileReq.FileID,
                FileSize = fileReq.FileSize,
                CreationTimeStamp = fileReq.CreationTimeStamp,
                OwnerID = fileReq.OwnerID,
                GroupID = fileReq.GroupID,
                OwnerPermissionRWX = (int)fileReq.OwnerPermissionRWX,
                GroupPermissionRWX = (int)fileReq.GroupPermissionRWX,
                GlobalPermissionRWX = (int)fileReq.GlobalPermissionRWX,
                ServerOperationID = (int)fileReq.ServerOperationID,
                CreateDt = DateTime.UtcNow
            };

            db?.Files?.Add(newFile);

            if (fileReq.fileAttributesDTO?.Description != null)
            {
                FileAttributes newFileAttributes = new()
                {
                    FileID = fileReq.FileID,
                    FileName = fileReq.FileName,
                    Description = fileReq.fileAttributesDTO.Description,
                    LastChangedTimeStamp = (int)fileReq.fileAttributesDTO.LastChangedTimeStamp,
                    LastChangedByUserID = (int)fileReq.fileAttributesDTO.LastChangedByUserID,
                    NumberAccesses = (int)fileReq.fileAttributesDTO.NumberAccesses,
                    StreamableFlag = (int)fileReq.fileAttributesDTO.StreamableFlag,
                    StreamingDataRate = (int)fileReq.fileAttributesDTO.StreamingDataRate,
                    CreateDt = DateTime.UtcNow
                };

                db?.FileAttributes?.Add(newFileAttributes);
            }

            db?.SaveChanges();
            return "File Added";
        }
        #endregion

        #region AddFile
        public async Task<dynamic?> addFile(FileDTO fileReq)
        {
            Files? existingFile = db?.Files?.Where(af => af.AppId == fileReq.AppId
                && af.FileName == fileReq.FileName).FirstOrDefault();
            FileAttributes? existingFileAttributes = db?.FileAttributes?.Where(af => af.AppId == fileReq.AppId
                && af.FileName == fileReq.FileName
                && af.Description == af.Description).FirstOrDefault();

            if (existingFile == null && existingFileAttributes == null)
            {
                Files newFile = new()
                {
                    AppId = fileReq.AppId,
                    FileName = fileReq.FileName,
                    ServerChecksum = fileReq.ServerChecksum,
                    FileID = fileReq.FileID,
                    FileSize = fileReq.FileSize,
                    CreationTimeStamp = fileReq.CreationTimeStamp,
                    OwnerID = fileReq.OwnerID,
                    GroupID = fileReq.GroupID,
                    OwnerPermissionRWX = (int)fileReq.OwnerPermissionRWX,
                    GroupPermissionRWX = (int)fileReq.GroupPermissionRWX,
                    GlobalPermissionRWX = (int)fileReq.GlobalPermissionRWX,
                    ServerOperationID = (int)fileReq.ServerOperationID,
                    CreateDt = DateTime.UtcNow,
                    ModifiedDt = DateTime.UtcNow,
                };

                db.Entry(newFile).State = EntityState.Added;
                db.Files?.Add(newFile);

                if (fileReq.fileAttributesDTO?.Description != null)
                {
                    FileAttributes newFileAttributes = new()
                    {
                        AppId = fileReq.AppId,
                        FileID = fileReq.FileID,
                        FileName = fileReq.FileName,
                        Description = fileReq.fileAttributesDTO.Description,
                        LastChangedTimeStamp = (int)fileReq.fileAttributesDTO.LastChangedTimeStamp,
                        LastChangedByUserID = (int)fileReq.fileAttributesDTO.LastChangedByUserID,
                        NumberAccesses = (int)fileReq.fileAttributesDTO.NumberAccesses,
                        StreamableFlag = (int)fileReq.fileAttributesDTO.StreamableFlag,
                        StreamingDataRate = (int)fileReq.fileAttributesDTO.StreamingDataRate,
                        CreateDt = DateTime.UtcNow,
                        ModifiedDt = DateTime.UtcNow,
                    };

                    db.Entry(newFileAttributes).State = EntityState.Added;
                    db.FileAttributes?.Add(newFileAttributes);
                }

                db.SaveChanges();
                return "File Added";
            }

            return null;
        }
        #endregion

        #region DeleteFile
        public async Task<dynamic?> deleteFile(FileDTO fileReq)
        {
            Files? existingFile = db?.Files?.Where(af => af.FileName == fileReq.FileName).FirstOrDefault();

            FileAttributes? existingFileAttributes = db?.FileAttributes?.Where(af => /*af.AppId == fileReq.AppId*/
                 af.FileName == fileReq.FileName).FirstOrDefault();

            if (existingFile != null)
            {
                db?.Files?.Remove(existingFile);
                db.Entry(existingFile).State = EntityState.Deleted;

                if (existingFileAttributes != null)
                {
                    db?.FileAttributes?.Remove(existingFileAttributes);
                    db.Entry(existingFileAttributes).State = EntityState.Deleted;
                }

                db?.SaveChanges();
                return "File Deleted";
            }

            return null;
        }
        #endregion

        #region getFileListExt
        public async Task<List<FileDTO>?> getFileListExt(int AppId, string FileNameBeginsWith, int OwnerByID, string metaKey)
        {
            List<FileDTO> files = new();

            List<Files>? filesListReturned = new();

            if (OwnerByID == -1 || OwnerByID < 0)
            {
                CustomLogger.LoggerAccessor.LogInfo($"OwnerById < 0");

                if (FileNameBeginsWith.Contains('*'))
                {
                    CustomLogger.LoggerAccessor.LogInfo($"1 FileNameBeginsWith: {FileNameBeginsWith}");
                    filesListReturned = db?.Files?.Where(File => File.AppId == AppId).ToList();
                }
                else
                {
                    CustomLogger.LoggerAccessor.LogInfo($"2 FileNameBeginsWith: {FileNameBeginsWith}");
                    filesListReturned = db?.Files?.Where(File => File.AppId == AppId &&
                        File.FileName.StartsWith(FileNameBeginsWith)).ToList();
                }

                if (filesListReturned == null)
                {
                    CustomLogger.LoggerAccessor.LogWarn("No Files found to list");
                    return null;
                }

                foreach (var file in filesListReturned)
                {

                    FileDTO fileToList = new()
                    {
                        AppId = AppId,
                        FileID = file.FileID,
                        ServerChecksum = file.ServerChecksum,
                        FileName = file.FileName,
                        FileSize = file.FileSize,
                        CreationTimeStamp = file.CreationTimeStamp,
                        OwnerID = file.OwnerID,
                        GroupID = file.GroupID,
                        OwnerPermissionRWX = (uint)file.OwnerPermissionRWX,
                        GroupPermissionRWX = (uint)file.GroupPermissionRWX,
                        GlobalPermissionRWX = (uint)file.GlobalPermissionRWX,
                        ServerOperationID = (uint)file.ServerOperationID,
                        CreateDt = file.CreateDt,
                    };
                    files.Add(fileToList);
                }

                return files;
            }
            else
            {
                filesListReturned = db?.Files?.Where(File => File.AppId == AppId
                            && File.FileName.StartsWith(FileNameBeginsWith)
                            && File.OwnerID == OwnerByID).ToList();

                if (filesListReturned == null)
                {
                    CustomLogger.LoggerAccessor.LogInfo("No Files found to list");
                    return null;
                }

                foreach (var file in filesListReturned)
                {

                    FileDTO fileToList = new()
                    {
                        AppId = AppId,
                        FileID = file.FileID,
                        ServerChecksum = file.ServerChecksum,
                        FileName = file.FileName,
                        FileSize = file.FileSize,
                        CreationTimeStamp = file.CreationTimeStamp,
                        OwnerID = file.OwnerID,
                        GroupID = file.GroupID,
                        OwnerPermissionRWX = (uint)file.OwnerPermissionRWX,
                        GroupPermissionRWX = (uint)file.GroupPermissionRWX,
                        GlobalPermissionRWX = (uint)file.GlobalPermissionRWX,
                        ServerOperationID = (uint)file.ServerOperationID,
                        CreateDt = file.CreateDt,
                    };
                    files.Add(fileToList);
                }

                return files;
            }
        }
        #endregion

        #region getFileList 
        public async Task<dynamic?> getFileList(int AppId, string FileNameBeginsWith, int OwnerByID)
        {
            List<FileDTO> filesReturn = new();

            //Jak X
            if (FileNameBeginsWith.Contains('*'))
            {
                var filesListReturnedByOwnerId = db?.Files?.Where(File => File.AppId == AppId
                            && File.OwnerID == OwnerByID).FirstOrDefault();

                var fileAttributes = db?.FileAttributes?.Where(x => x.AppId == AppId)
                    .FirstOrDefault();

                if (filesListReturnedByOwnerId == null)
                {
                    CustomLogger.LoggerAccessor.LogWarn("No Files found to list");
                    return null;
                }
                else if (fileAttributes == null)
                {
                    CustomLogger.LoggerAccessor.LogWarn("No Files attributes found to list");
                    return null;
                }

                FileDTO fileToList = new()
                {
                    AppId = AppId,
                    FileID = filesListReturnedByOwnerId.FileID,
                    ServerChecksum = filesListReturnedByOwnerId.ServerChecksum,
                    FileName = filesListReturnedByOwnerId.FileName,
                    FileSize = filesListReturnedByOwnerId.FileSize,
                    CreationTimeStamp = filesListReturnedByOwnerId.CreationTimeStamp,
                    OwnerID = filesListReturnedByOwnerId.OwnerID,
                    GroupID = filesListReturnedByOwnerId.GroupID,
                    OwnerPermissionRWX = (uint)filesListReturnedByOwnerId.OwnerPermissionRWX,
                    GroupPermissionRWX = (uint)filesListReturnedByOwnerId.GroupPermissionRWX,
                    GlobalPermissionRWX = (uint)filesListReturnedByOwnerId.GlobalPermissionRWX,
                    ServerOperationID = (uint)filesListReturnedByOwnerId.ServerOperationID,
                    fileAttributesDTO = new FileAttributesDTO()
                    {
                        AppId = AppId,
                        FileID = fileAttributes.FileID,
                        FileName = filesListReturnedByOwnerId.FileName,
                        Description = fileAttributes.Description,
                        LastChangedTimeStamp = (uint)fileAttributes.LastChangedTimeStamp,
                        LastChangedByUserID = (uint)fileAttributes.LastChangedByUserID,
                        NumberAccesses = (uint)fileAttributes.NumberAccesses,
                        StreamableFlag = (uint)fileAttributes.StreamableFlag,
                        StreamingDataRate = (uint)fileAttributes.StreamingDataRate,
                        CreateDt = fileAttributes.CreateDt,
                    }
                };
                filesReturn.Add(fileToList);

                return filesReturn;
            }
            if (AppId == 10994 || AppId == 11203 || AppId == 11204)
            {
                CustomLogger.LoggerAccessor.LogInfo($"JakX Filtering");

                List<Files>? filesListReturned = db?.Files?.Where(File => File.AppId == AppId
                    && File.FileName.StartsWith(FileNameBeginsWith)).ToList();


                List<FileAttributes>? fileAttributes = db?.FileAttributes?.Where(x => x.AppId == AppId
                    && x.FileName.StartsWith(FileNameBeginsWith)).ToList();

                if (filesListReturned == null)
                {
                    CustomLogger.LoggerAccessor.LogWarn("No Files found to list");
                    return null;
                }
                else if (fileAttributes == null)
                {
                    CustomLogger.LoggerAccessor.LogWarn("No Files attributes found to list");
                    return null;
                }


                foreach (var file in filesListReturned)
                {
                    FileAttributesDTO fileAttributesDTO = new();

                    foreach (var fileAttribute in fileAttributes)
                    {

                        fileAttributesDTO = new FileAttributesDTO()
                        {
                            AppId = AppId,
                            FileID = file.FileID,
                            FileName = file.FileName,
                            Description = fileAttribute.Description,
                            LastChangedTimeStamp = (uint)fileAttribute.LastChangedTimeStamp,
                            LastChangedByUserID = (uint)fileAttribute.LastChangedByUserID,
                            NumberAccesses = (uint)fileAttribute.NumberAccesses,
                            StreamableFlag = (uint)fileAttribute.StreamableFlag,
                            StreamingDataRate = (uint)fileAttribute.StreamingDataRate,
                            CreateDt = fileAttribute.CreateDt,
                        };
                    }


                    filesReturn.Add(new FileDTO()
                    {
                        AppId = AppId,
                        FileID = file.FileID,
                        ServerChecksum = file.ServerChecksum,
                        FileName = file.FileName,
                        FileSize = file.FileSize,
                        CreationTimeStamp = file.CreationTimeStamp,
                        OwnerID = file.OwnerID,
                        GroupID = file.GroupID,
                        OwnerPermissionRWX = (uint)file.OwnerPermissionRWX,
                        GroupPermissionRWX = (uint)file.GroupPermissionRWX,
                        GlobalPermissionRWX = (uint)file.GlobalPermissionRWX,
                        ServerOperationID = (uint)file.ServerOperationID,
                        fileAttributesDTO = new FileAttributesDTO()
                        {
                            AppId = AppId,
                            FileID = fileAttributesDTO.FileID,
                            FileName = fileAttributesDTO.FileName,
                            Description = fileAttributesDTO.Description,
                            LastChangedTimeStamp = fileAttributesDTO.LastChangedTimeStamp,
                            LastChangedByUserID = fileAttributesDTO.LastChangedByUserID,
                            NumberAccesses = fileAttributesDTO.NumberAccesses,
                            StreamableFlag = fileAttributesDTO.StreamableFlag,
                            StreamingDataRate = fileAttributesDTO.StreamingDataRate,
                            CreateDt = fileAttributesDTO.CreateDt,
                        },
                        CreateDt = file.CreateDt,
                    });
                }

                if (filesReturn == null)
                    return null;

                CustomLogger.LoggerAccessor.LogInfo($"filesReturn: {filesReturn.Count()}");

                return filesReturn;
            }

            else if (OwnerByID < 0)
            {
                CustomLogger.LoggerAccessor.LogInfo($"OwnerById < 0");
                var filesListReturned = db?.Files?.Where(File => File.AppId == AppId
                    && File.FileName.StartsWith(FileNameBeginsWith))
                    .FirstOrDefault();

                var fileAttributes = db?.FileAttributes?.Where(x => x.AppId == AppId
                    && x.FileName.StartsWith(FileNameBeginsWith))
                    .FirstOrDefault();

                if (filesListReturned == null)
                {
                    CustomLogger.LoggerAccessor.LogInfo("No Files found to list");
                    return null;
                }
                else if (fileAttributes == null)
                {
                    CustomLogger.LoggerAccessor.LogInfo("No Files attributes found to list");
                    return null;
                }

                FileDTO fileToList = new()
                {
                    AppId = AppId,
                    FileID = filesListReturned.FileID,
                    ServerChecksum = filesListReturned.ServerChecksum,
                    FileName = filesListReturned.FileName,
                    FileSize = filesListReturned.FileSize,
                    CreationTimeStamp = filesListReturned.CreationTimeStamp,
                    OwnerID = filesListReturned.OwnerID,
                    GroupID = filesListReturned.GroupID,
                    OwnerPermissionRWX = (uint)filesListReturned.OwnerPermissionRWX,
                    GroupPermissionRWX = (uint)filesListReturned.GroupPermissionRWX,
                    GlobalPermissionRWX = (uint)filesListReturned.GlobalPermissionRWX,
                    ServerOperationID = (uint)filesListReturned.ServerOperationID,
                    CreateDt = filesListReturned.CreateDt,
                    fileAttributesDTO = new FileAttributesDTO()
                    {
                        AppId = AppId,
                        FileID = filesListReturned.FileID,
                        FileName = filesListReturned.FileName,
                        Description = fileAttributes.Description,
                        LastChangedTimeStamp = (uint)fileAttributes.LastChangedTimeStamp,
                        LastChangedByUserID = (uint)fileAttributes.LastChangedByUserID,
                        NumberAccesses = (uint)fileAttributes.NumberAccesses,
                        StreamableFlag = (uint)fileAttributes.StreamableFlag,
                        StreamingDataRate = (uint)fileAttributes.StreamingDataRate,
                        CreateDt = fileAttributes.CreateDt,
                    }
                };
                filesReturn.Add(fileToList);
                return filesReturn;
            }
            else
            {
                var filesListReturnedByOwnerId = db?.Files?.Where(File => File.AppId == AppId
                            && File.FileName.StartsWith(FileNameBeginsWith)
                            && File.OwnerID == OwnerByID).FirstOrDefault();

                var fileAttributes = db?.FileAttributes?.Where(x => x.AppId == AppId
                    && x.FileName.StartsWith(FileNameBeginsWith))
                    .FirstOrDefault();

                if (filesListReturnedByOwnerId == null)
                {
                    CustomLogger.LoggerAccessor.LogWarn("No Files found to list");
                    return null;
                }
                else if (fileAttributes == null)
                {
                    CustomLogger.LoggerAccessor.LogWarn("No Files attributes found to list");
                    return null;
                }

                FileDTO fileToList = new()
                {
                    AppId = AppId,
                    FileID = filesListReturnedByOwnerId.FileID,
                    ServerChecksum = filesListReturnedByOwnerId.ServerChecksum,
                    FileName = filesListReturnedByOwnerId.FileName,
                    FileSize = filesListReturnedByOwnerId.FileSize,
                    CreationTimeStamp = filesListReturnedByOwnerId.CreationTimeStamp,
                    OwnerID = filesListReturnedByOwnerId.OwnerID,
                    GroupID = filesListReturnedByOwnerId.GroupID,
                    OwnerPermissionRWX = (uint)filesListReturnedByOwnerId.OwnerPermissionRWX,
                    GroupPermissionRWX = (uint)filesListReturnedByOwnerId.GroupPermissionRWX,
                    GlobalPermissionRWX = (uint)filesListReturnedByOwnerId.GlobalPermissionRWX,
                    ServerOperationID = (uint)filesListReturnedByOwnerId.ServerOperationID,
                    fileAttributesDTO = new FileAttributesDTO()
                    {
                        AppId = AppId,
                        FileID = fileAttributes.FileID,
                        FileName = filesListReturnedByOwnerId.FileName,
                        Description = fileAttributes.Description,
                        LastChangedTimeStamp = (uint)fileAttributes.LastChangedTimeStamp,
                        LastChangedByUserID = (uint)fileAttributes.LastChangedByUserID,
                        NumberAccesses = (uint)fileAttributes.NumberAccesses,
                        StreamableFlag = (uint)fileAttributes.StreamableFlag,
                        StreamingDataRate = (uint)fileAttributes.StreamingDataRate,
                        CreateDt = fileAttributes.CreateDt,
                    }
                };
                filesReturn.Add(fileToList);

                return filesReturn;
            }
        }
        #endregion

        #region updateFileAttributes
        public async Task<dynamic?> updateFileAttributes(FileAttributesDTO request)
        {
            var fileAttributes = db?.FileAttributes?.FirstOrDefault(x => x.FileID == request.FileID);

            if (fileAttributes == null)
                return null;

            db?.FileAttributes?.Attach(fileAttributes);

            FileAttributes newFileAttributes = new()
            {
                Description = request.Description,
                NumberAccesses = (int)request.NumberAccesses,
                LastChangedTimeStamp = (int)request.LastChangedTimeStamp,
                LastChangedByUserID = (int)request.LastChangedByUserID,
                StreamableFlag = (int)request.StreamableFlag,
                StreamingDataRate = (int)request.StreamingDataRate,
                CreateDt = DateTime.UtcNow,
            };
            db?.FileAttributes?.Add(newFileAttributes);
            db?.SaveChanges();

            return "File changed";
        }
        #endregion

        #region getFileAttributes
        public async Task<dynamic?> getFileAttributes(FileDTO fileAttrReq)
        {
            return db?.FileAttributes?.Where(x => x.FileName == fileAttrReq.FileName);
        }
        #endregion

        #region updateFileMetaData
        public async Task<dynamic> updateFileMetaData(FileMetaDataDTO request)
        {
            FileMetaData? fileMetaData = db?.FileMetaDatas?.Where(x => x.Key == request.Key &&
                                                        x.AppId == request.AppId).FirstOrDefault();

            if (fileMetaData == null)
            {
                FileMetaData addFileMetaData = new()
                {
                    AppId = request.AppId,
                    FileName = request.FileName,
                    Key = request.Key,
                    Value = request.Value,
                    CreateDt = request.CreateDt,
                };
                db.Entry(addFileMetaData).State = EntityState.Added;
                db.FileMetaDatas?.Add(addFileMetaData);

                db.SaveChanges();
                return "File Metadata added";
            }
            else
            {
                fileMetaData.AppId = request.AppId;
                fileMetaData.FileName = request.FileName;
                fileMetaData.Key = request.Key;
                fileMetaData.Value = request.Value;
                fileMetaData.ModifiedDt = DateTime.UtcNow;

                db.Entry(fileMetaData).State = EntityState.Modified;
                db.FileMetaDatas?.Attach(fileMetaData);

                db.SaveChanges();
                return "File Metadata updated";
            }
        }
        #endregion

        #region getFileMetaData
        public async Task<dynamic?> getFileMetaData(int appid, string fileName, string Key)
        {
            if (Key == null)
                return db?.FileMetaDatas?.Where(x => x.AppId == appid
                       && x.FileName == fileName).ToList();
            else
                return db?.FileMetaDatas?.Where(x => x.AppId == appid
                        && x.FileName == fileName
                        && x.Key == Key).ToList();
        }
        #endregion
    }
}
