using CakeShopApi.Models;

namespace CakeShopApi.Services;
public interface ITokenService {
    string CreateToken(User user);
}
