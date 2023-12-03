using HTTPServer.API.JUGGERNAUT.farm.animal;
using HTTPServer.API.JUGGERNAUT.farm.crafting;
using HTTPServer.API.JUGGERNAUT.farm.furniture;
using HTTPServer.API.JUGGERNAUT.farm.plant;

namespace HTTPServer.API.JUGGERNAUT
{
    public class JUGGERNAUTClass : IDisposable
    {
        string absolutepath;
        string method;
        private bool disposedValue;

        public JUGGERNAUTClass(string method, string absolutepath)
        {
            this.absolutepath = absolutepath;
            this.method = method;
        }

        public string? ProcessRequest(Dictionary<string, string>? QueryParameters, byte[]? PostData = null, string? ContentType = null)
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
                            return clearasil.joinedspace.ProcessJoinedSpace(QueryParameters);
                        case "/clearasil/getscores.php":
                            return clearasil.getscores.ProcessGetScores(QueryParameters);
                        case "/clearasil/pushrewards.php":
                            return clearasil.pushrewards.ProcessPushRewards(QueryParameters);
                        case "/clearasil/pushtime.php":
                            return clearasil.pushtime.ProcessPushTime(QueryParameters);
                        case "/clearasil/pushscore.php":
                            return clearasil.pushscore.ProcessPushScore(QueryParameters);
                        case "/farm/crafting_getxpstats.php":
                            return crafting_getxpstats.ProcessGetXpStats();
                        case "/farm/crafting_stats.php":
                            return crafting_stats.ProcessGetStats();
                        case "/farm/furniture_crafting_up.php":
                            return furniture_crafting_up.ProcessCraftingUp(QueryParameters);
                        case "/farm/furniture_slot_saved.php":
                            return furniture_slot_saved.ProcessSlotSaved(QueryParameters);
                        case "/farm/furniture_crafting_crafted.php":
                            return furniture_crafting_crafted.ProcessCraftingCrafted(QueryParameters);
                        case "/farm/furniture_crafting_down.php":
                            return furniture_crafting_down.ProcessCraftingDown(QueryParameters);
                        case "/farm/furniture_crafting_sold.php":
                            return furniture_crafting_sold.ProcessCraftingSold(QueryParameters);
                        case "/farm/resources_getall.php":
                            return farm.resources_getall.ProcessGetAll(QueryParameters);
                        case "/farm/weather_up.php":
                            return farm.weather_up.ProcessWeatherUp(QueryParameters);
                        case "/farm/wood_earned.php":
                            return farm.wood_earned.ProcessWoodEarned(QueryParameters);
                        case "/farm/remodel_getall.php":
                            return farm.remodel_getall.ProcessGetAll(QueryParameters);
                        case "/farm/animal_getall.php":
                            return animal_getall.ProcessGetAll(QueryParameters);
                        case "/farm/plant_getall.php":
                            return plant_getall.ProcessGetAll(QueryParameters);
                        case "/farm/plant_getxpstats.php":
                            return plant_getxpstats.ProcessGetXpStats();
                        case "/farm/plant_pushxp.php":
                            return plant_pushxp.ProcessPushXp(QueryParameters);
                        case "/farm/plant_bought.php":
                            return plant_bought.ProcessBought(QueryParameters);
                        case "/farm/plant_leveled.php":
                            return plant_leveled.ProcessLeveled(QueryParameters);
                        case "/farm/plant_watered.php":
                            return plant_watered.ProcessWatered(QueryParameters);
                        case "/farm/plant_getxp.php":
                            return plant_getxp.ProcessGetXp(QueryParameters);
                        case "/farm/plant_sold.php":
                            return plant_sold.ProcessSold(QueryParameters);
                        case "/farm/plant_stats.php":
                            return plant_stats.ProcessStats();
                        case "/farm/animal_bought.php":
                            return animal_bought.ProcessBought(QueryParameters);
                        case "/farm/animal_sold.php":
                            return animal_sold.ProcessSold(QueryParameters);
                        case "/farm/animal_leveled.php":
                            return animal_leveled.ProcessLeveled(QueryParameters);
                        case "/farm/animal_fed.php":
                            return animal_fed.ProcessFed(QueryParameters);
                        case "/farm/animal_renewed.php":
                            return animal_renewed.ProcessRenewed(QueryParameters);
                        case "/farm/animal_collect_renew.php":
                            return animal_collect_renew.ProcessCollectRenew(QueryParameters);
                        case "/farm/animal_stats.php":
                            return animal_stats.ProcessStats();
                        case "/farm/furniture_down.php":
                            return furniture_down.ProcessDown(QueryParameters);
                        case "/cutteridge/effects2012chances.php":
                            return cutteridge.effects2012chances.ProcessChances();
                        default:
                            break;
                    }
                    break;
                case "POST":
                    switch (absolutepath)
                    {
                        case "/farm/furniture_up.php":
                            return furniture_up.ProcessUp(PostData, ContentType);
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
