using System.Net;
using WatsonWebserver.Core;

namespace ApacheNet
{
    internal static class PostAuthParameters
    {
        public static void Build(WebserverBase server)
        {
            #region PS Home
            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/objects/D2CDD8B2-DE444593-A64C68CB-0B5EDE23/{id}.xml", async (ctx) =>
            {
                string? QuizID = ctx.Request.Url.Parameters["id"];

                if (!string.IsNullOrEmpty(QuizID))
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                    ctx.Response.ContentType = "text/xml";
                    await ctx.Response.Send("<Root></Root>"); // TODO - Figure out this complicated LUAC in object : D2CDD8B2-DE444593-A64C68CB-0B5EDE23
                }
                else
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.Send();
                }
            });

            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/static/Lockwood/Features/Venue/{scenetype}/{build}/{country}/setDressing.xml", async (ctx) =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                ctx.Response.ContentType = "text/xml";
                await ctx.Response.Send(WebAPIService.OHS.LUA2XmlProcessor.TransformLuaTableToXml(@"setDressing = {
		                profiles = {
			                'Votertron',
			                'Customisation',
			                'Posertrons',
			                'BackstagePass',
		                },
		                entities = {
			                ['main_scene'] = {
				                'Booth_1_Colour',
				                'Booth_1_Back',
				                'Booth_2_Colour',
				                'Booth_2_Back',
				                'Booth_3_Colour',
				                'Booth_3_Back',
				                'Booths_Outer',
				                'Lobby_Gate_Internal',
				                'Lobby_Gate_External',
				                'Lobby_Gate_Base',
				                'Lobby_Main_Walls',
				                'Lobby_Short_Walls',
				                'Lobby_Tall_Walls',
				                --'Lobby_Bench_Top',
				                --'Lobby_Bench_Bottom',
				                'Bar_Top_Front',
				                'Bar_Back',
				                'Bar_Front',
				                'Bar_Floor',
				                --'Bar_Sofa_Bottom',
				                --'Bar_Sofa_Top',
				                'Vip_Back',
				                'Vip_Floor_Tile',
				                'Vip_StairWall',
				                'Vip_Cloth',
				                --'Vip_Back_Sofa',
				                --'Vip_Sofa_Bottom',
				                --'Vip_Sofa_Top',
				                'Catwalk_Floor',
				                'Catwalk_backboard',
				                --'Catwalk_Scoreboard',
				                'Backstage_Back_Wall',
				                'Backstage_Walls',
				                'Backstage_Ceiling',
				                'Backstage_Floor',
				                --'Backstage_Sofa_Bottom',
				                --'Backstage_Sofa_Top',
				                'Backstage_Tassles',
				                'Backstage_wood_dark_veneer',
				                'Backstage_wood_dark_veneer2',
			                }
		                }
	                }

		            local options = {
			            profiles = {},
			            entities = {},
		            }
		            if setDressing then
			            for _, name in ipairs(setDressing.profiles) do
				            options.profiles[name] = {}
			            end
		            end
		            if  setDressing then
			            for entName, entDef in pairs(setDressing.entities) do
				            options.entities[entName] = {}
				            for _, matName in ipairs(entDef) do
					            options.entities[entName][matName] = {}
				            end
			            end
		            end

                    local TableFromInput = {
					    options 	= options,
					    setups 		= {
						    ['default'] = {
							    profiles 	= {},
							    dressing 	= {
								    entities 	= {},
							    },
						    },
					    },
					    schedule 	= {
						    january 	= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    february 	= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    march 		= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    april 		= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    may 		= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    june 		= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    july 		= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    august 		= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    september 	= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    october 	= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    november 	= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
						    december 	= {'default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default','default'},
					    },
					    customisationGroups 	= {},
					    customisation 			= {
						    profiles 	= {},
						    entities 	= {},
					    }
				    }

                    return XmlConvert.LuaToXml(TableFromInput, 'root', 1)
                    "));
            });

            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/static/Lockwood/Features/Venue/{scenetype}/{build}/{country}/camPath.xml", async (ctx) =>
            {
                // For now this returns a default table, TODO: feed this with actual data.
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                ctx.Response.ContentType = "text/xml";
                await ctx.Response.Send("<root></root>");
            });
            #endregion
        }
    }
}
