using WebAPIService.JUGGERNAUT.farm.animal;
using WebAPIService.JUGGERNAUT.farm.crafting;
using WebAPIService.JUGGERNAUT.farm.furniture;
using WebAPIService.JUGGERNAUT.farm.plant;

namespace WebAPIService.JUGGERNAUT
{
    public class JUGGERNAUTClass : IDisposable
    {
        private string absolutepath;
        private string method;
        private bool disposedValue;

        public JUGGERNAUTClass(string method, string absolutepath)
        {
            this.absolutepath = absolutepath;
            this.method = method;
        }

        public string? ProcessRequest(Dictionary<string, string>? QueryParameters, byte[]? PostData = null, string? ContentType = null, string apiPath = null)
        {
            if (string.IsNullOrEmpty(absolutepath))
                return null;

            switch (method)
            {
                case "GET":
                    switch (absolutepath)
                    {
                        case "/clearasil/pushtelemetry.php":
                            if (QueryParameters != null)
                            {
                                string? user = QueryParameters["user"];
                                string? timeingame = QueryParameters["timeingame"];
                                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(timeingame))
                                {
                                    try
                                    {
                                        int time = int.Parse(timeingame);
                                        CustomLogger.LoggerAccessor.LogInfo($"[JUGGERNAUT] - User: {user} spent {time / 60}:{time % 60} minutes in clearasil.");
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                                return string.Empty;
                            }
                            break;
                        case "/clearasil/joinedspace.php":
                            return clearasil.joinedspace.ProcessJoinedSpace(QueryParameters, apiPath);
                        case "/clearasil/getscores.php":
                            return clearasil.getscores.ProcessGetScores(QueryParameters, apiPath);
                        case "/clearasil/pushrewards.php":
                            return clearasil.pushrewards.ProcessPushRewards(QueryParameters, apiPath);
                        case "/clearasil/pushtime.php":
                            return clearasil.pushtime.ProcessPushTime(QueryParameters, apiPath);
                        case "/clearasil/pushscore.php":
                            return clearasil.pushscore.ProcessPushScore(QueryParameters, apiPath);
                        case "/farm/crafting_getxpstats.php":
                            return crafting_getxpstats.ProcessGetXpStats();
                        case "/farm/crafting_stats.php":
                            return crafting_stats.ProcessGetStats(apiPath);
                        case "/farm/furniture_crafting_up.php":
                            return furniture_crafting_up.ProcessCraftingUp(QueryParameters, apiPath);
                        case "/farm/furniture_slot_saved.php":
                            return furniture_slot_saved.ProcessSlotSaved(QueryParameters, apiPath);
                        case "/farm/furniture_crafting_crafted.php":
                            return furniture_crafting_crafted.ProcessCraftingCrafted(QueryParameters, apiPath);
                        case "/farm/furniture_crafting_down.php":
                            return furniture_crafting_down.ProcessCraftingDown(QueryParameters, apiPath);
                        case "/farm/furniture_crafting_sold.php":
                            return furniture_crafting_sold.ProcessCraftingSold(QueryParameters, apiPath);
                        case "/farm/resources_getall.php":
                            return farm.resources_getall.ProcessGetAll(QueryParameters, apiPath);
                        case "/farm/weather_up.php":
                            return farm.weather_up.ProcessWeatherUp(QueryParameters, apiPath);
                        case "/farm/wood_earned.php":
                            return farm.wood_earned.ProcessWoodEarned(QueryParameters, apiPath);
                        case "/farm/remodel_getall.php":
                            return farm.remodel_getall.ProcessGetAll(QueryParameters, apiPath);
                        case "/farm/animal_getall.php":
                            return animal_getall.ProcessGetAll(QueryParameters, apiPath);
                        case "/farm/plant_getall.php":
                            return plant_getall.ProcessGetAll(QueryParameters, apiPath);
                        case "/farm/plant_getxpstats.php":
                            return plant_getxpstats.ProcessGetXpStats();
                        case "/farm/plant_pushxp.php":
                            return plant_pushxp.ProcessPushXp(QueryParameters, apiPath);
                        case "/farm/plant_bought.php":
                            return plant_bought.ProcessBought(QueryParameters, apiPath);
                        case "/farm/plant_leveled.php":
                            return plant_leveled.ProcessLeveled(QueryParameters, apiPath);
                        case "/farm/plant_watered.php":
                            return plant_watered.ProcessWatered(QueryParameters, apiPath);
                        case "/farm/plant_getxp.php":
                            return plant_getxp.ProcessGetXp(QueryParameters, apiPath);
                        case "/farm/plant_sold.php":
                            return plant_sold.ProcessSold(QueryParameters, apiPath);
                        case "/farm/plant_stats.php":
                            return plant_stats.ProcessStats(apiPath);
                        case "/farm/animal_bought.php":
                            return animal_bought.ProcessBought(QueryParameters, apiPath);
                        case "/farm/animal_sold.php":
                            return animal_sold.ProcessSold(QueryParameters, apiPath);
                        case "/farm/animal_leveled.php":
                            return animal_leveled.ProcessLeveled(QueryParameters, apiPath);
                        case "/farm/animal_fed.php":
                            return animal_fed.ProcessFed(QueryParameters, apiPath);
                        case "/farm/animal_renewed.php":
                            return animal_renewed.ProcessRenewed(QueryParameters, apiPath);
                        case "/farm/animal_collect_renew.php":
                            return animal_collect_renew.ProcessCollectRenew(QueryParameters, apiPath);
                        case "/farm/animal_stats.php":
                            return animal_stats.ProcessStats(apiPath);
                        case "/farm/furniture_down.php":
                            return furniture_down.ProcessDown(QueryParameters, apiPath);
                        case "/cutteridge/effects2012chances.php":
                            return cutteridge.effects2012chances.ProcessChances(apiPath);
                        default:
                            break;
                    }
                    break;
                case "POST":
                    switch (absolutepath)
                    {
                        case "/farm/furniture_up.php":
                            return furniture_up.ProcessUp(PostData, ContentType, apiPath);
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    absolutepath = string.Empty;
                    method = string.Empty;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~JUGGERNAUTClass()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
