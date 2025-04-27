using CustomLogger;
using System;
using System.IO;
using System.Net;
using WatsonWebserver.Core;

namespace ApacheNet.Extensions.Lockwood
{
    internal static class Venue
    {
        private static UniqueIDGenerator UniqueIDCounter = new UniqueIDGenerator(0);

        public static void BuildVenuePlugin(WebserverBase server)
        {
            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/static/Lockwood/Features/Venue/{scenetype}/{build}/{country}/setDressing.xml", async (ctx) =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                ctx.Response.ContentType = "text/xml";
                string xmlPath = $"/static/Lockwood/Features/Venue/{ctx.Request.Url.Parameters["scenetype"]}/{ctx.Request.Url.Parameters["build"]}/{ctx.Request.Url.Parameters["country"]}/setDressing.xml";
                string filePath = !ApacheNetServerConfiguration.DomainFolder ? ApacheNetServerConfiguration.HTTPStaticFolder + xmlPath : ApacheNetServerConfiguration.HTTPStaticFolder + $"/{ctx.Request.RetrieveHeaderValue("Host")}" + xmlPath;

                if (File.Exists(filePath))
                {
                    await ctx.Response.Send(File.ReadAllText(filePath));
                    return;
                }

                LoggerAccessor.LogDebug($"[PostAuthParameters] - setDressing data was not found for the Venue, falling back to server file.");

                await ctx.Response.Send(WebAPIService.OHS.LUA2XmlProcessor.TransformLuaTableToXml(@"
                    local maps = {
	                    'colourMap', 'normalMap', 'specularMap', 'envMap', 'emissiveMap', 'colour2Map', 'normal2Map', 'specular2Map'
                    }

                    setDressing = {
		                profiles = {
			                'Votertron',
			                'Customisation',
			                'Posertrons',
			                'BackstagePass',
		                },
		                entities = {
                            ['main_scene'] = {
                                ['Booth_1_Colour']             = { ['default'] = {} },
                                ['Booth_1_Back']               = { ['default'] = {} },
                                ['Booth_2_Colour']             = { ['default'] = {} },
                                ['Booth_2_Back']               = { ['default'] = {} },
                                ['Booth_3_Colour']             = { ['default'] = {} },
                                ['Booth_3_Back']               = { ['default'] = {} },
                                ['Booths_Outer']               = { ['default'] = {} },
                                ['Lobby_Gate_Internal']        = { ['default'] = {} },
                                ['Lobby_Gate_External']        = { ['default'] = {} },
                                ['Lobby_Gate_Base']            = { ['default'] = {} },
                                ['Lobby_Main_Walls']           = { ['default'] = {} },
                                ['Lobby_Short_Walls']          = { ['default'] = {} },
                                ['Lobby_Tall_Walls']           = { ['default'] = {} },
                                ['Bar_Top_Front']              = { ['default'] = {} },
                                ['Bar_Back']                   = { ['default'] = {} },
                                ['Bar_Front']                  = { ['default'] = {} },
                                ['Bar_Floor']                  = { ['default'] = {} },
                                ['Vip_Back']                   = { ['default'] = {} },
                                ['Vip_Floor_Tile']             = { ['default'] = {} },
                                ['Vip_StairWall']              = { ['default'] = {} },
                                ['Vip_Cloth']                  = { ['default'] = {} },
                                ['Catwalk_Floor']              = { ['default'] = {} },
                                ['Catwalk_backboard']          = { ['default'] = {} },
                                ['Backstage_Back_Wall']        = { ['default'] = {} },
                                ['Backstage_Walls']            = { ['default'] = {} },
                                ['Backstage_Ceiling']          = { ['default'] = {} },
                                ['Backstage_Floor']            = { ['default'] = {} },
                                ['Backstage_Tassles']          = { ['default'] = {} },
                                ['Backstage_wood_dark_veneer'] = { ['default'] = {} },
                                ['Backstage_wood_dark_veneer2']= { ['default'] = {} }
                            }
                        }
	                }

		            local options = {
			            profiles = {},
			            entities = {},
		            }
                    local dressingDef = {}

		            if setDressing then
			            for _, name in ipairs(setDressing.profiles) do
				            options.profiles[name] = {}
			            end
		            end
		            if setDressing then
                        for entName, entDef in pairs(setDressing.entities) do
                            options.entities[entName] = {}
                            for matName, variants in pairs(entDef) do
                                options.entities[entName][matName] = {}
                                for optionId, optionData in pairs(variants) do
                                    options.entities[entName][matName][optionId] = optionData
                                end
                            end
                        end
                    end
                    if setDressing then
                        for entName, entDef in pairs(setDressing.entities) do
                            dressingDef[entName] = {}
                            for matName, variants in pairs(entDef) do
                                dressingDef[entName][matName] = {}
                                for optionId, _ in pairs(variants) do
                                    dressingDef[entName][matName] = optionId
                                end
                            end
                        end
                    end

                    local TableFromInput = {
					    options 	= options,
					    setups 		= {
						     ['default'] = {
							        profiles 	= {
                                        ['Votertron'] = 'dancefloor',
                                        ['Customisation'] = 'default',
                                        ['Posertrons'] = 'default',
                                        ['BackstagePass'] = 'default',
                                        ['Music'] = 'LKWD',
                                    },
							        dressing 	= {
								        entities 	= dressingDef,
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

                    return XmlConvert.LuaToXml(TableFromInput, 'lua', 1)
                    "));
            });

            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/static/Lockwood/Features/Venue/{scenetype}/{build}/{country}/features.xml", async (ctx) =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                ctx.Response.ContentType = "text/xml";
                string xmlPath = $"/static/Lockwood/Features/Venue/{ctx.Request.Url.Parameters["scenetype"]}/{ctx.Request.Url.Parameters["build"]}/{ctx.Request.Url.Parameters["country"]}/features.xml";
                string filePath = !ApacheNetServerConfiguration.DomainFolder ? ApacheNetServerConfiguration.HTTPStaticFolder + xmlPath : ApacheNetServerConfiguration.HTTPStaticFolder + $"/{ctx.Request.RetrieveHeaderValue("Host")}" + xmlPath;

                if (File.Exists(filePath))
                {
                    await ctx.Response.Send(File.ReadAllText(filePath));
                    return;
                }

                LoggerAccessor.LogDebug($"[PostAuthParameters] - features data was not found for the Venue, falling back to server file.");

                await ctx.Response.Send(WebAPIService.OHS.LUA2XmlProcessor.TransformLuaTableToXml(@"
                            local TableFromInput = {
                                ['Votertron'] = {
                                    dancefloor = true
                                },
                                ['Customisation'] = {
                                    default = true
                                },
                                ['Posertrons'] = {
                                    default = true
                                },
                                ['BackstagePass'] = {
                                    default = true
                                },
                                ['Music'] = {
                                    Stitchkins = true,
                                    Nightclub = true,
                                    IronFusion = true,
                                    LKWD = true,
                                    Medusa = true,
                                    Corporate = true,
                                    Drey = true,
                                    Wings = true,
                                    Fool_Throttle = true,
                                    Delirious_Squid = true,
                                },
				            }

                            return XmlConvert.LuaToXml(TableFromInput, 'lua', 1)
                            "));
            });

            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/static/Lockwood/Features/Venue/{scenetype}/{build}/{country}/camPath.xml", async (ctx) =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                ctx.Response.ContentType = "text/xml";
                string xmlPath = $"/static/Lockwood/Features/Venue/{ctx.Request.Url.Parameters["scenetype"]}/{ctx.Request.Url.Parameters["build"]}/{ctx.Request.Url.Parameters["country"]}/camPath.xml";
                string filePath = !ApacheNetServerConfiguration.DomainFolder ? ApacheNetServerConfiguration.HTTPStaticFolder + xmlPath : ApacheNetServerConfiguration.HTTPStaticFolder + $"/{ctx.Request.RetrieveHeaderValue("Host")}" + xmlPath;

                if (File.Exists(filePath))
                {
                    await ctx.Response.Send(File.ReadAllText(filePath));
                    return;
                }

                LoggerAccessor.LogDebug($"[PostAuthParameters] - camPath data was not found for the Venue, falling back to server file.");

                await ctx.Response.Send("<lua></lua>");
            });

            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/static/Lockwood/Features/Venue/{scenetype}/{build}/{country}/{group_def}/{profile_def}", async (ctx) =>
            {
                string? group_def = ctx.Request.Url.Parameters["group_def"];
                string? profile_def = ctx.Request.Url.Parameters["profile_def"];
                if (string.IsNullOrEmpty(group_def) || string.IsNullOrEmpty(profile_def))
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.Send();
                    return;
                }
                const string param_str = ".param_group";
                if (!profile_def.EndsWith(param_str))
                {
                    LoggerAccessor.LogWarn($"[PostAuthParameters] - profile_def definition path was invalid!");
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.Send();
                    return;
                }
                string xmlPath = $"/static/Lockwood/Features/Venue/{ctx.Request.Url.Parameters["scenetype"]}/{ctx.Request.Url.Parameters["build"]}/{ctx.Request.Url.Parameters["country"]}/{group_def}/{profile_def}";
                profile_def = profile_def.Substring(0, profile_def.Length - param_str.Length);
                if (string.IsNullOrEmpty(profile_def))
                {
                    LoggerAccessor.LogWarn($"[PostAuthParameters] - profile_def definition project was invalid!");
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.Send();
                    return;
                }
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                ctx.Response.ContentType = "text/xml";
                string filePath = !ApacheNetServerConfiguration.DomainFolder ? ApacheNetServerConfiguration.HTTPStaticFolder + xmlPath : ApacheNetServerConfiguration.HTTPStaticFolder + $"/{ctx.Request.RetrieveHeaderValue("Host")}" + xmlPath;

                if (File.Exists(filePath))
                {
                    await ctx.Response.Send(File.ReadAllText(filePath));
                    return;
                }

                LoggerAccessor.LogDebug($"[PostAuthParameters] - {profile_def} data was not found for the Venue, falling back to server file.");

                switch (group_def)
                {
                    case "Music":
                        await ctx.Response.Send($@"<lua>
	                                <feature_com_context type='num'>{UniqueIDCounter.CreateUniqueID()}</feature_com_context>
                                    <soundStream>
		                                <{profile_def}></{profile_def}>
	                                </soundStream>
	                                <feature_root>
		                                <root>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>0,0,0,0</scale>
                                            <pos type='vec'>0,0,0,0</pos>
                                            <soundStream>
			                                    <name>{profile_def}</name>
                                                <override>
                                                    <channel>{profile_def}_channel</channel>
                                                    <stream>stream_{profile_def}</stream>
                                                    <innerRadius type='num'>50000</innerRadius>
                                                    <outerRadius type='num'>50000</outerRadius>
                                                    <crossfadeTime type='num'>0.5</crossfadeTime>
                                                    <volume type='num'>1</volume>
                                                    <canTerminate type='bool'>false</canTerminate>
                                                    <mode>2D</mode>
                                                </override>
		                                    </soundStream>
		                                </root>
	                                </feature_root>
                                </lua>");
                        break;
                    case "Posertrons":
                        await ctx.Response.Send($@"<lua>
	                                <feature_com_context type='num'>{UniqueIDCounter.CreateUniqueID()}</feature_com_context>
                                    <applet>
		                                <Posertrons_V2></Posertrons_V2>
	                                </applet>
	                                <feature_root>
		                                <root>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>1,1,1,1</scale>
                                            <pos type='vec'>0,0,0,0</pos>
                                            <applet>
			                                    <name>Posertrons_V2</name>
                                                <override>
                                                    <appletId>Posertron_Applet</appletId>
                                                    <register>AppletRegister_posertron.lua</register>
                                                    <params>
                                                        <model>
                                                            <mdl>catwalk.mdl</mdl>
                                                            <hkx>catwalk.hkx</hkx>
                                                        </model>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root>
	                                </feature_root>
                                </lua>");
                        break;
                    default:
                        LoggerAccessor.LogWarn($"[PostAuthParameters] - group_def definition data was not found for group_def:{group_def}!");
                        break;
                }
                await ctx.Response.Send("<lua></lua>");
            });
        }
    }
}
