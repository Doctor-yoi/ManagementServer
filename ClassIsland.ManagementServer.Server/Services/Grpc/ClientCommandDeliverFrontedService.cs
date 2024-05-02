using ClassIsland.Core.Protobuf.Client;
using ClassIsland.Core.Protobuf.Enum;
using ClassIsland.Core.Protobuf.Server;
using ClassIsland.Core.Protobuf.Service;
using ClassIsland.ManagementServer.Server.Entities;
using ClassIsland.ManagementServer.Server.Enums;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace ClassIsland.ManagementServer.Server.Services.Grpc;

public class ClientCommandDeliverFrontedService(ClientCommandDeliverService clientCommandDeliverService, ILogger<ClientCommandDeliverFrontedService> logger) : ClientCommandDeliver.ClientCommandDeliverBase
{
    private ClientCommandDeliverService ClientCommandDeliverService { get; } = clientCommandDeliverService;

    private ILogger<ClientCommandDeliverFrontedService> Logger { get; } = logger;
    
    public override async Task ListenCommand(ClientCommandDeliverScReq request, IServerStreamWriter<ClientCommandDeliverScRsp> responseStream, ServerCallContext context)
    {
        ClientCommandDeliverService.Streams[request.ClientUid] = responseStream;
        Logger.LogInformation("与 {} 建立命令流连接", request.ClientUid);
        await ClientCommandDeliverService.DeliverCommandAsync(CommandTypes.ServerConnected, new Empty(),
            new ObjectsAssignee()
            {
                AssigneeType = (int)AssigneeTypes.ClientUid,
                TargetClientCuid = request.ClientUid
            });
        await Task.Run(() => context.CancellationToken.WaitHandle.WaitOne());
        ClientCommandDeliverService.Streams.Remove(ClientCommandDeliverService.Streams.FirstOrDefault(x => x.Value == responseStream).Key);
        Logger.LogInformation("断开与 {} 命令流连接", request.ClientUid);
    }
}