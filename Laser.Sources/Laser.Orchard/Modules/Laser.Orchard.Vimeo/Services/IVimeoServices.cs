using Laser.Orchard.Vimeo.Models;
using Laser.Orchard.Vimeo.ViewModels;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.Vimeo.Services {
    public interface IVimeoServices : IDependency {
        //bool Create(VimeoSettingsPartViewModel settings);
        //VimeoSettingsPartViewModel GetByToken(string aToken);
        //VimeoSettingsPartViewModel Get();
        void UpdateSettings(VimeoSettingsPartViewModel vm);

        bool TokenIsValid(VimeoSettingsPartViewModel vm);

        bool GroupIsValid(VimeoSettingsPartViewModel vm);
        bool AlbumIsValid(VimeoSettingsPartViewModel vm);
        bool ChannelIsValid(VimeoSettingsPartViewModel vm);

        

        //call these methods to properly start an upload
        //TODO: replace these with a single method
        int IsValidFileSize(int fileSize);
        string GenerateUploadTicket(int uploadId);
        int GenerateNewMediaPart(int uploadId);

        //these methods terminate an upload
        VerifyUploadResults VerifyUpload(int mediaPartId);
        bool TerminateUpload(int mediaPartId);

        //task methods
        //Should we really expose them here? as things stand, they can be called from the task handler (which is fine) 
        //as well as from the admin controller and the upload controller, where we don't need them. We could separate
        //the method definitions in different interfaces to make things more clean as the code grows larger.
        void ScheduleUploadVerification();
        void ScheduleVideoCompletion();
        int VerifyAllUploads();
        int TerminateUploads();

        string DestroyUpload(int mediaPartId);

#if DEBUG
        //these methods here don't need to be exposed, but they are easier to test and debug if they may be called directly.
        bool TokenIsValid(string aToken);
        bool GroupIsValid(string gName, string aToken);
        bool AlbumIsValid(string aName, string aToken);
        bool ChannelIsValid(string cName, string aToken);

        VimeoUploadQuota CheckQuota();
        int UsedQuota();
        int FreeQuota();

        string PatchVideo(int ucId, string name = "", string description = "");
        string TryAddVideoToGroup(int ucId);
        string TryAddVideoToChannel(int ucId);
        string TryAddVideoToAlbum(int ucId);

        string ExtractVimeoStreamURL(int ucId);

        void FinishMediaPart(int ucId);

        string GetVideoStatus(int ucId);

        void ClearRepositoryTables();
#endif

        
    }

}
