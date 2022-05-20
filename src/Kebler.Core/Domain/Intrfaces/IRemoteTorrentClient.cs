using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Core.Models;
using Kebler.Core.Models.Arguments;
using Kebler.Core.Models.Entities;

namespace Kebler.Core.Domain.Intrfaces
{
    public interface IRemoteTorrentClient
    {
        public Task<TransmissionResponse<SessionInfo>> GetSessionInformationAsync(CancellationToken token);

        public Task<TransmissionResponse<Statistic>> GetSessionStatisticAsync(CancellationToken token);

        public Task<TransmissionResponse<TransmissionTorrents>> TorrentGetAsync(string[] fields, CancellationToken token);

        public Task<TransmissionResponse<SessionSettings>> GetSessionSettingsAsync(CancellationToken token);

        public Task<TransmissionResponse> TorrentSetLocationAsync(IEnumerable<uint> ids, string location, bool move,
            CancellationToken token);

        public Task<TransmissionResponse> TorrentStopAsync(uint[] ids, CancellationToken token);

        public Task<AddTorrentResponse> TorrentAddAsync(NewTorrent torrent, CancellationToken token);

        public Task<TransmissionResponse> TorrentStartAsync(uint[] ids, CancellationToken token);

        public Task<TransmissionResponse> TorrentVerifyAsync(uint[] ids, CancellationToken token);

        public Task<TransmissionResponse> ReannounceTorrentsAsync(uint[] ids, CancellationToken token);

        public Task<TransmissionResponse> TorrentQueueMoveTopAsync(uint[] ids, CancellationToken token);

        public Task<TransmissionResponse> TorrentQueueMoveUpAsync(uint[] ids, CancellationToken token);

        public Task<TransmissionResponse> TorrentQueueMoveDownAsync(uint[] ids, CancellationToken token);

        public Task<TransmissionResponse> TorrentQueueMoveBottomAsync(uint[] ids, CancellationToken token);

        public Task<Enums.RemoveResult> TorrentRemoveAsync(uint[] ids, CancellationToken token, bool deleteData = false);

        public Task<TransmissionResponse> TorrentSetAsync(TorrentSettings settings, CancellationToken token);

        public Task<TransmissionResponse> SetSessionSettingsAsync(SessionSettings settings, CancellationToken token);

        public Task<TransmissionTorrents> TorrentGetAsyncWithId(string[] fields, CancellationToken token, params uint[] ids);

        public Task<TransmissionResponse<RenameTorrentInfo>> TorrentRenamePathAsync(uint id, string path, string name, CancellationToken token);
    }
}
