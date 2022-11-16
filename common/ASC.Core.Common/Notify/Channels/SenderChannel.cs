// (c) Copyright Ascensio System SIA 2010-2022
//
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
//
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
//
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
//
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
//
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
//
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

namespace ASC.Notify.Channels;

public class SenderChannel : ISenderChannel
{
    private readonly ISink _firstSink;
    private readonly ISink _senderSink;

    public string SenderName { get; private set; }


    public SenderChannel(DispatchEngine dispatchEngine, string senderName, ISink decorateSink, ISink senderSink)
    {
        SenderName = senderName ?? throw new ArgumentNullException(nameof(senderName));
        _firstSink = decorateSink;
        _senderSink = senderSink ?? throw new ApplicationException($"channel with tag {senderName} not created sender sink");


        var dispatcherSink = new DispatchSink(SenderName, dispatchEngine);
        _firstSink = AddSink(_firstSink, dispatcherSink);
    }

    public async Task SendAsync(INoticeMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        await _firstSink.ProcessMessageAsync(message);
    }

    public async Task<SendResponse> DirectSend(INoticeMessage message)
    {
        return await _senderSink.ProcessMessage(message);
    }

    private ISink AddSink(ISink firstSink, ISink addedSink)
    {
        if (firstSink == null)
        {
            return addedSink;
        }

        if (addedSink == null)
        {
            return firstSink;
        }

        var current = firstSink;
        while (current.NextSink != null)
        {
            current = current.NextSink;
        }
        current.NextSink = addedSink;

        return firstSink;
    }
}
