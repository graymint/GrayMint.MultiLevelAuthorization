using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class AppConverter
{
    public static App ToDto(this AppModel appModel)
    {
        return new App
        {
            AppId = appModel.AppId
        };
    }
}