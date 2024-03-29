﻿using KanbanTasker.Extensions;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace KanbanTasker.Helpers.MicrosoftGraph.Requests
{
    public class OneDriveRequests : IRequest
    {
        public GraphServiceClient GraphClient { get; set; }

        public OneDriveRequests(GraphServiceClient graphServiceClient)
        {
            // Initializes the client used to make calls to the Microsoft Graph API
            GraphClient = graphServiceClient;
        }

        /// <summary>
        /// Get current user's OneDrive root folder.
        /// </summary>
        /// <returns>A DriveItem representing the root folder.</returns>
        public async Task<DriveItem> GetRootAsync()
        {
            try
            {
                // GET /me/drive/root
                return await GraphClient.Me.Drive.Root.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, Error getting signed-in users one drive root: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the children of the current user's OneDrive root folder.
        /// </summary>
        /// <returns>A collection of DriveItems.</returns>
        public async Task<IDriveItemChildrenCollectionPage> GetRootChildrenAsync()
        {
            try
            {
                // GET /me/drive/root/children 
                return await GraphClient.Me.Drive.Root.Children.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, Error getting signed-in users one drive root children: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the specified folder as a DriveItem. 
        /// </summary>
        /// <param name="folderPath">Path to the data file starting from your local application folder.</param>
        /// <returns>A DriveItem representing the specified folder. Returns null if folder doesn't exist.</returns>
        public async Task<DriveItem> GetFolderAsync(string folderPath)
        {
            try
            {
                // GET /me/drive/root/{folderPath} 
                var searchCollection = await GraphClient.Me.Drive.Root.Search("Kanban Tasker").Request().GetAsync();
                foreach (var folder in searchCollection)
                    if (folder.Name == "Kanban Tasker")
                        return folder;
                return null;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.BadGateway)
                {
                    Console.WriteLine($"Service Exception, Bad Gateway. Error getting signed-in users one drive folder: {ex.Message}");
                }
                else if (ex.IsMatch(GraphErrorCode.GeneralException.ToString()))
                {
                    Console.WriteLine($"General Exception, error getting folder. Please check internet connection.");
                }
                throw;
            }
        }

        /// <summary>
        /// Creates a new folder in the current user's OneDrive root folder.
        /// </summary>
        /// <param name="folderName">Name of the folder to create.</param>
        /// <returns>A DriveItem representing the newly created Folder.</returns>
        public async Task<DriveItem> CreateNewFolderAsync(string folderName)
        {
            try
            {
                var driveItem = new DriveItem
                {

                    Name = folderName,
                    Folder = new Folder
                    {
                    },
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"@microsoft.graph.conflictBehavior","fail"}
                    }
                };

                return await GraphClient.Me.Drive.Root.ItemWithPath("Applications").Children
                    .Request()
                    .AddAsync(driveItem);
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, error creating folder in signed-in users one drive root: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Uploads a file in the current user's OneDrive root folder from this applications local folder
        /// using its specified itemId and filename.
        /// </summary>
        /// <param name="itemId">Unique item identifier within a DriveItem (folder/file).</param>
        /// <param name="filename">Name of the datafile.</param>
        /// <returns>A DriveItem representing the newly uploaded file.</returns>
        public async Task<DriveItem> UploadFileAsync(string itemId, string filename)
        {
            try
            {
                Windows.Storage.StorageFolder storageFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;

                Windows.Storage.StorageFile sampleFile =
                    await storageFolder.GetFileAsync(filename);

                var stream = await sampleFile.OpenStreamForReadAsync();

                return await GraphClient.Me.Drive.Items[itemId].ItemWithPath(filename).Content
                    .Request()
                    .PutAsync<DriveItem>(stream);
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Expception, Error uploading file to signed-in users one drive: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Restores the applications data using a backup from the current user's OneDrive. 
        /// <para>Note: Application requires restart after restoring data</para>
        /// </summary>
        /// <param name="itemId">Unique item identifier within a DriveItem (i.e., a folder/file facet).</param>
        /// <param name="filename">Name of the datafile.</param>
        /// <returns></returns>
        public async Task RestoreFileAsync(string itemId, string dataFilename)
        {
            try
            {
                // Local storage folder
                Windows.Storage.StorageFolder storageFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;

                // Our local ktdatabase.db file
                Windows.Storage.StorageFile originalDataFile =
                    await storageFolder.GetFileAsync(dataFilename);

                // Stream for the backed up data file
                var backedUpFileStream = await GraphClient.Me.Drive.Items[itemId]
                    .ItemWithPath(dataFilename)
                    .Content
                    .Request()
                    .GetAsync();

                // Backed up file
                var backedUpFile = await storageFolder.CreateFileAsync("temp", CreationCollisionOption.ReplaceExisting);
                var newStream = await backedUpFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                // Write data to new file
                using (var outputStream = newStream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                    {
                        var buffer = backedUpFileStream.ToByteArray();
                        dataWriter.WriteBytes(buffer);

                        await dataWriter.StoreAsync();
                        await outputStream.FlushAsync();
                    }
                }

                // Copy and replace local file
                await backedUpFile.CopyAsync(storageFolder, dataFilename, NameCollisionOption.ReplaceExisting);
            }

            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Forbidden)
                    Console.WriteLine($"Access Denied: {ex.Message}");

                Console.WriteLine($"Service Exception, Error uploading file to signed-in users one drive: {ex.Message}");
                // return null;
            }
        }
    }
}