using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public class AfCodeService : IAfCodeService
    {
        private readonly IIndexedDbRepo<Bk, string> _bkRepository;

        public AfCodeService(
            IIndexedDbRepo<Bk, string> bkRepository)
        {
            _bkRepository = bkRepository;
        }

        public async Task<string> CreateAfCodeAsync(string url, AfCodeType codeType)
        {
            var bk = await _bkRepository.GetAsync(url);
            if (bk == null)
            {
                return string.Empty;
            }

            var code = new AfCode
            {
                Title = bk.Title,
                Url = bk.Url,
                Tags = bk.Tags?.ToArray() ?? Array.Empty<string>(),
            };
            switch (codeType)
            {
                case AfCodeType.JsonBase64:
                    {
                        var json = JsonSerializer.Serialize(code);
                        var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                        return CreateCode(base64String);
                    }
                case AfCodeType.CompressionJsonBase64:
                    {
                        var json = JsonSerializer.Serialize(code);
                        var input = Encoding.UTF8.GetBytes(json).AsMemory();
                        var readOnlyMemory = Compress(input);
                        var base64String = Convert.ToBase64String(readOnlyMemory.Span);
                        return CreateCode(base64String);
                    }
                case AfCodeType.Cloud:
                    throw new NotImplementedException();
                case AfCodeType.PlainText:
                    {
                        var json = JsonSerializer.Serialize(code);
                        return CreateCode(json);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(codeType), codeType, null);
            }

            string CreateCode(string payload)
            {
                return $"{Consts.AfCodeSchemaPrefix}{codeType:D}{payload}";
            }
        }

        public Task<bool> TryParseAsync(string source, out AfCodeResult? afCodeResult)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                afCodeResult = null;
                return Task.FromResult(false);
            }

            if (!source.StartsWith(Consts.AfCodeSchemaPrefix))
            {
                afCodeResult = null;
                return Task.FromResult(false);
            }

            var payload = source[Consts.AfCodeSchemaPrefix.Length..];
            var codeTypeStr = payload[..1];
            if (!int.TryParse(codeTypeStr, out var codeTypeInt))
            {
                afCodeResult = null;
                return Task.FromResult(false);
            }

            var codeType = (AfCodeType)codeTypeInt;
            payload = payload[1..];

            switch (codeType)
            {
                case AfCodeType.JsonBase64:
                    {
                        var json = Convert.FromBase64String(payload);
                        var afCode = JsonSerializer.Deserialize<AfCode>(json);
                        if (!string.IsNullOrEmpty(afCode?.Url))
                        {
                            afCodeResult = new AfCodeResult
                            {
                                Tags = afCode.Tags!,
                                Title = afCode.Title!,
                                Url = afCode.Url,
                                AfCodeType = AfCodeType.JsonBase64
                            };
                            return Task.FromResult(true);
                        }
                    }
                    break;
                case AfCodeType.CompressionJsonBase64:
                    {
                        var gzipBytes = Convert.FromBase64String(payload);
                        var utf8Json = Decompress(gzipBytes);
                        var json = Encoding.UTF8.GetString(utf8Json.Span);
                        var afCode = JsonSerializer.Deserialize<AfCode>(json);
                        if (!string.IsNullOrEmpty(afCode?.Url))
                        {
                            afCodeResult = new AfCodeResult
                            {
                                Tags = afCode.Tags!,
                                Title = afCode.Title!,
                                Url = afCode.Url,
                                AfCodeType = AfCodeType.JsonBase64
                            };
                            return Task.FromResult(true);
                        }
                    }
                    break;
                case AfCodeType.PlainText:
                    {
                        var afCode = JsonSerializer.Deserialize<AfCode>(payload);
                        if (!string.IsNullOrEmpty(afCode?.Url))
                        {
                            afCodeResult = new AfCodeResult
                            {
                                Tags = afCode.Tags!,
                                Title = afCode.Title!,
                                Url = afCode.Url,
                                AfCodeType = AfCodeType.JsonBase64
                            };
                            return Task.FromResult(true);
                        }
                    }
                    break;
                case AfCodeType.Cloud:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            afCodeResult = null;
            return Task.FromResult(false);
        }

        private static ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> input)
        {
            using var compressStream = new MemoryStream();
            using var compressor = new GZipStream(compressStream, CompressionMode.Compress);
            compressor.Write(input.Span);
            compressor.Flush();
            compressor.Close();
            return compressStream.ToArray();
        }

        private static ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> input)
        {
            using var sourceStream = new MemoryStream();
            using var decompressStream = new MemoryStream();
            using var decompressor = new GZipStream(sourceStream, CompressionMode.Decompress);
            sourceStream.Write(input.Span);
            sourceStream.Seek(0, SeekOrigin.Begin);
            decompressor.CopyTo(decompressStream);
            return decompressStream.ToArray();
        }
    }
}