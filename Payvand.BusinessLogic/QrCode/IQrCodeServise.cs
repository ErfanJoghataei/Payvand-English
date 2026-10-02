using Microsoft.AspNetCore.Http;
using Payvand.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Payvand.BusinessLogic.QrCode
{
    public interface IQrCodeServise
    {
        Task<byte[]> CreateQrCodeWithLogoAsync(
        int? shortenedLinkId,
        string link,
        string foregroundColor,
        string backgroundColor,
        int size,
        byte[]? logoBytes);

        Task<QRCodeEntity>
            SaveAsync(int? shortenedLinkId,
         string link,
        string foregroundColor,
        string backgroundColor,
        int size,
        byte[]? logoBytes,
       int userId);

        
    }
}
