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
                    case "BackstagePass":
                        await ctx.Response.Send($@"<lua>
	                                    <feature_com_context type='num'>{UniqueIDCounter.CreateUniqueID()}</feature_com_context>
                                        <applet>
                                            <StageEnter_def></StageEnter_def>
		                                    <StageExit_def></StageExit_def>
	                                    </applet>
                                        <detectors>
                                            <detectorsStageEnter_def></detectorsStageEnter_def>
		                                    <detectorsStageExit_def></detectorsStageExit_def>
	                                    </detectors>
	                                    <feature_root>
                                            <root_stage_enter>
			                                    <rot type='vec'>0,0,0,0</rot>
                                                <scale type='vec'>0,0,0,0</scale>
                                                <pos type='vec'>7.617,1.532,-20.761,0</pos>
                                                <applet>
			                                        <name>StageEnter_def</name>
                                                    <override>
                                                        <appletId>StageAccess_Applet</appletId>
                                                        <register>AppletRegister_StageAccess.lua</register>
                                                        <params>
                                                            <destination>stage_enter_dest</destination>
                                                            <mode>Entrance</mode>
                                                        </params>
                                                    </override>
		                                        </applet>
                                                <detectors>
			                                            <name>detectorsStageEnter_def</name>
                                                        <override>
                                                            <proximity>50000</proximity>
                                                            <homeTarget>
                                                                <localisation>STAGE_ENTER</localisation>
                                                                 <radius type='num'>1.2</radius>
                                                            </homeTarget>
                                                        </override>
		                                        </detectors>
		                                    </root_stage_enter>
                                            <root_stage_exit>
			                                    <rot type='vec'>0,0,0,0</rot>
                                                <scale type='vec'>0,0,0,0</scale>
                                                <pos type='vec'>6.907,1.532,-20.761,0</pos>
                                                <applet>
			                                        <name>StageEnter_def</name>
                                                    <override>
                                                        <appletId>StageAccess_Applet</appletId>
                                                        <register>AppletRegister_StageAccess.lua</register>
                                                        <params>
                                                            <destination>stage_exit_dest</destination>
                                                            <mode>Exit</mode>
                                                        </params>
                                                    </override>
		                                        </applet>
                                                <detectors>
			                                            <name>detectorsStageExit_def</name>
                                                        <override>
                                                            <proximity>50000</proximity>
                                                            <homeTarget>
                                                                <localisation>STAGE_LEAVE</localisation>
                                                                <radius type='num'>1.2</radius>
                                                            </homeTarget>
                                                        </override>
		                                        </detectors>
		                                    </root_stage_exit>
                                            <stage_enter_dest>
			                                        <rot type='vec'>0,0,0,0</rot>
                                                    <scale type='vec'>0,0,0,0</scale>
                                                    <pos type='vec'>6.766,1.922,-20.612,0</pos>
		                                    </stage_enter_dest>
                                            <stage_exit_dest>
			                                        <rot type='vec'>0,0,0,0</rot>
                                                    <scale type='vec'>0,0,0,0</scale>
                                                    <pos type='vec'>10.369,1.922,-20.612,0</pos>
		                                    </stage_exit_dest>
	                                    </feature_root>
                                    </lua>");
                        return;
                    case "Customisation":
                        await ctx.Response.Send($@"<lua>
	                                    <feature_com_context type='num'>{UniqueIDCounter.CreateUniqueID()}</feature_com_context>
                                        <applet>
		                                    <Customisation_def></Customisation_def>
	                                    </applet>
                                        <detectors>
		                                    <detectorsCustomisation_def></detectorsCustomisation_def>
	                                    </detectors>
	                                    <feature_root>
                                            <root>
			                                    <rot type='vec'>0,0,0,0</rot>
                                                <scale type='vec'>0,0,0,0</scale>
                                                <pos type='vec'>-2.366,1.790,16.434,0</pos>
                                                <applet>
			                                        <name>Customisation_def</name>
                                                    <override>
                                                        <appletId>ProfileCustomisation_Applet</appletId>
                                                        <register>AppletRegister_ProfileCustomisation.lua</register>
                                                        <params>
                                                            <url>generic_cp</url>
                                                        </params>
                                                    </override>
		                                        </applet>
                                                <detectors>
			                                        <name>detectorsCustomisation_def</name>
                                                    <override>
                                                        <proximity>50000</proximity>
                                                        <homeTarget>
                                                            <localisation>CUSTOMISE</localisation>
                                                            <radius type='num'>2.5</radius>
                                                        </homeTarget>
                                                    </override>
		                                        </detectors>
		                                    </root>
	                                    </feature_root>
                                    </lua>");
                        return;
                    case "Votertron":
                        await ctx.Response.Send($@"<lua>
	                                <feature_com_context type='num'>{UniqueIDCounter.CreateUniqueID()}</feature_com_context>
                                    <applet>
		                                <Cameratron_def></Cameratron_def>
		                                <Screenatron_def></Screenatron_def>
		                                <Screenatron_0_def></Screenatron_0_def>
		                                <Screenatron_1_def></Screenatron_1_def>
		                                <Screenatron_2_def></Screenatron_2_def>
		                                <Stagertron_def></Stagertron_def>
		                                <Strutertron_def></Strutertron_def>
		                                <Votertron_def></Votertron_def>
		                                <Votertron_0_def></Votertron_0_def>
		                                <Votertron_1_def></Votertron_1_def>
	                                </applet>
                                    <detectors>
		                                    <detectorsStrut_def></detectorsStrut_def>
		                                    <detectorsVotertron_def></detectorsVotertron_def>
		                                    <detectorsVotertron_0_def></detectorsVotertron_0_def>
		                                    <detectorsVotertron_1_def></detectorsVotertron_1_def>
	                                </detectors>
	                                <feature_root>
		                                <root_cam>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>0,0,0,0</scale>
                                            <pos type='vec'>0,0,0,0</pos>
                                            <applet>
			                                    <name>Cameratron_def</name>
                                                <override>
                                                    <appletId>Cameratron_Applet</appletId>
                                                    <register>AppletRegister_cameratron.lua</register>
                                                    <params>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_cam>
                                        <root_screen>
			                                <rot type='vec'>0,-90,0,-90</rot>
                                            <scale type='vec'>0.8,1.6,1,1</scale>
                                            <pos type='vec'>16.360,4.748,2.837,0</pos>
                                            <applet>
			                                    <name>Screenatron_def</name>
                                                <override>
                                                    <appletId>Screenatron_Applet</appletId>
                                                    <register>AppletRegister_screenatron.lua</register>
                                                    <params>
                                                        <scale type='num'>2</scale>
                                                        <mode>multiple</mode>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_screen>
                                        <root_screen_large_0>
			                                <rot type='vec'>0,-90,0,-90</rot>
                                            <scale type='vec'>0.8,1.6,1,1</scale>
                                            <pos type='vec'>16.360,4.748,-6.308,0</pos>
                                            <applet>
			                                    <name>Screenatron_def</name>
                                                <override>
                                                    <appletId>Screenatron_Applet</appletId>
                                                    <register>AppletRegister_screenatron.lua</register>
                                                    <params>
                                                        <scale type='num'>2</scale>
                                                        <mode>multiple</mode>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_screen_large_0>
                                        <root_screen_large_1>
			                                <rot type='vec'>0,-90,0,-90</rot>
                                            <scale type='vec'>0.8,1.6,1,1</scale>
                                            <pos type='vec'>16.360,4.748,11.750,0</pos>
                                            <applet>
			                                    <name>Screenatron_def</name>
                                                <override>
                                                    <appletId>Screenatron_Applet</appletId>
                                                    <register>AppletRegister_screenatron.lua</register>
                                                    <params>
                                                        <scale type='num'>2</scale>
                                                        <mode>multiple</mode>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_screen_large_1>
                                        <root_screen_0>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>0.7,1.1,1,1</scale>
                                            <pos type='vec'>-4.606,3.940,-18.917,0</pos>
                                            <applet>
			                                    <name>Screenatron_0_def</name>
                                                <override>
                                                    <appletId>Screenatron_Applet</appletId>
                                                    <register>AppletRegister_screenatron.lua</register>
                                                    <params>
                                                        <scale type='num'>3</scale>
                                                        <voterId type='num'>1</voterId>
                                                        <mode>single</mode>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_screen_0>
                                        <root_screen_1>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>0.7,1.1,1,1</scale>
                                            <pos type='vec'>8.987,3.940,-18.917,0</pos>
                                            <applet>
			                                    <name>Screenatron_1_def</name>
                                                <override>
                                                    <appletId>Screenatron_Applet</appletId>
                                                    <register>AppletRegister_screenatron.lua</register>
                                                    <params>
                                                        <scale type='num'>3</scale>
                                                        <voterId type='num'>3</voterId>
                                                        <mode>single</mode>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_screen_1>
                                        <root_screen_2>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>0.5,1.1,1,1</scale>
                                            <pos type='vec'>2.128,3.940,-21.718,0</pos>
                                            <applet>
			                                    <name>Screenatron_2_def</name>
                                                <override>
                                                    <appletId>Screenatron_Applet</appletId>
                                                    <register>AppletRegister_screenatron.lua</register>
                                                    <params>
                                                        <scale type='num'>3</scale>
                                                        <voterId type='num'>2</voterId>
                                                        <mode>single</mode>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_screen_2>
                                        <root_stage>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>0,0,0,0</scale>
                                            <pos type='vec'>0,0,0,0</pos>
                                            <applet>
			                                    <name>Stagertron_def</name>
                                                <override>
                                                    <appletId>Stagertron_Applet</appletId>
                                                    <register>AppletRegister_stagertron.lua</register>
                                                    <params>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_stage>
                                        <root_struter>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>0,0,0,0</scale>
                                            <pos type='vec'>-3.960,1.479,-20.632,0</pos>
                                            <applet>
			                                    <name>Strutertron_def</name>
                                                <override>
                                                    <appletId>Strutertron_Applet</appletId>
                                                    <register>AppletRegister_strutertron.lua</register>
                                                    <params>
                                                        <malePos type='vec'>1.396,1.000,-20.253,0</malePos>
                                                        <femalePos type='vec'>1.396,1.000,-19.087,0</femalePos>
                                                        <destination>catwalk_exit_dest</destination>
                                                        <behavior>WalkTheWalk</behavior>
                                                        <female>votertron_female</female>
                                                        <male>votertron_male</male>
                                                    </params>
                                                </override>
		                                    </applet>
                                            <detectors>
			                                        <name>detectorsStrut_def</name>
                                                    <override>
                                                        <proximity>50000</proximity>
                                                        <homeTarget>
                                                            <localisation>VOTERTRON_JOIN_AS_POSER</localisation>
                                                            <radius type='num'>1.2</radius>
                                                        </homeTarget>
                                                    </override>
		                                    </detectors>
		                                </root_struter>
                                        <root_vote>
			                                <rot type='vec'>0,180,0,180</rot>
                                            <scale type='vec'>1,1,1,1</scale>
                                            <pos type='vec'>0.670,0.501,9.141,0</pos>
                                            <applet>
			                                    <name>Votertron_def</name>
                                                <override>
                                                    <appletId>Votertron_Applet</appletId>
                                                    <register>AppletRegister_Votertron.lua</register>
                                                    <params>
                                                        <male type='vec'>0,0,0,0</male>
                                                        <female type='vec'>0,0,0,0</female>
                                                        <voterId type='num'>1</voterId>
                                                    </params>
                                                </override>
		                                    </applet>
                                            <detectors>
			                                        <name>detectorsVotertron_def</name>
                                                    <override>
                                                        <proximity>50000</proximity>
                                                        <homeTarget>
                                                            <localisation>VOTERTRON_JOIN_AS_VOTER</localisation>
                                                            <radius type='num'>0.8</radius>
                                                        </homeTarget>
                                                    </override>
		                                    </detectors>
		                                </root_vote>
                                        <root_vote_0>
			                                <rot type='vec'>0,180,0,180</rot>
                                            <scale type='vec'>1,1,1,1</scale>
                                            <pos type='vec'>1.944,0.501,9.141,0</pos>
                                            <applet>
			                                    <name>Votertron_0_def</name>
                                                <override>
                                                    <appletId>Votertron_Applet</appletId>
                                                    <register>AppletRegister_Votertron.lua</register>
                                                    <params>
                                                        <male type='vec'>0,0,0,0</male>
                                                        <female type='vec'>0,0,0,0</female>
                                                        <voterId type='num'>2</voterId>
                                                    </params>
                                                </override>
		                                    </applet>
                                            <detectors>
			                                        <name>detectorsVotertron_0_def</name>
                                                    <override>
                                                        <proximity>50000</proximity>
                                                        <homeTarget>
                                                            <localisation>VOTERTRON_JOIN_AS_VOTER</localisation>
                                                            <radius type='num'>0.8</radius>
                                                        </homeTarget>
                                                    </override>
		                                    </detectors>
		                                </root_vote_0>
                                        <root_vote_1>
			                                <rot type='vec'>0,180,0,180</rot>
                                            <scale type='vec'>1,1,1,1</scale>
                                            <pos type='vec'>3.191,0.501,9.141,0</pos>
                                            <applet>
			                                    <name>Votertron_1_def</name>
                                                <override>
                                                    <appletId>Votertron_Applet</appletId>
                                                    <register>AppletRegister_Votertron.lua</register>
                                                    <params>
                                                        <male type='vec'>0,0,0,0</male>
                                                        <female type='vec'>0,0,0,0</female>
                                                        <voterId type='num'>3</voterId>
                                                    </params>
                                                </override>
		                                    </applet>
                                            <detectors>
			                                        <name>detectorsVotertron_1_def</name>
                                                    <override>
                                                        <proximity>50000</proximity>
                                                        <homeTarget>
                                                            <localisation>VOTERTRON_JOIN_AS_VOTER</localisation>
                                                            <radius type='num'>0.8</radius>
                                                        </homeTarget>
                                                    </override>
		                                    </detectors>
		                                </root_vote_1>
                                        <catwalk_exit_dest>
			                                    <rot type='vec'>0,0,0,0</rot>
                                                <scale type='vec'>0,0,0,0</scale>
                                                <pos type='vec'>-4.589,1.849,-20.478,0</pos>
		                                </catwalk_exit_dest>
	                                </feature_root>
                                </lua>");
                        return;
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
                        return;
                    case "Posertrons":
                        await ctx.Response.Send($@"<lua>
	                                <feature_com_context type='num'>{UniqueIDCounter.CreateUniqueID()}</feature_com_context>
                                    <applet>
		                                <Posertrons_V2></Posertrons_V2>
	                                </applet>
	                                <feature_root>
		                                <root_catwalk>
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
		                                </root_catwalk>
                                        <root_judge>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>1,1,1,1</scale>
                                            <pos type='vec'>2,0,8.077,0</pos>
                                            <applet>
			                                    <name>Posertrons_V2</name>
                                                <override>
                                                    <appletId>Posertron_Applet</appletId>
                                                    <register>AppletRegister_posertron.lua</register>
                                                    <params>
                                                        <model>
                                                            <mdl>judges.mdl</mdl>
                                                            <hkx>judges.hkx</hkx>
                                                        </model>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_judge>
                                        <root_judge_0>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>1,1,1,1</scale>
                                            <pos type='vec'>-2.863,0.169,-6.622,0</pos>
                                            <applet>
			                                    <name>Posertrons_V2</name>
                                                <override>
                                                    <appletId>Posertron_Applet</appletId>
                                                    <register>AppletRegister_posertron.lua</register>
                                                    <params>
                                                        <model>
                                                            <mdl>bench.mdl</mdl>
                                                            <hkx>bench.hkx</hkx>
                                                        </model>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_judge_0>
                                        <root_judge_1>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>1,1,1,1</scale>
                                            <pos type='vec'>7.117,0.169,-6.622,0</pos>
                                            <applet>
			                                    <name>Posertrons_V2</name>
                                                <override>
                                                    <appletId>Posertron_Applet</appletId>
                                                    <register>AppletRegister_posertron.lua</register>
                                                    <params>
                                                        <model>
                                                            <mdl>bench.mdl</mdl>
                                                            <hkx>bench.hkx</hkx>
                                                        </model>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_judge_1>
                                        <root_block>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>1,1,1,1</scale>
                                            <pos type='vec'>7.731,1.479,-20.632,0</pos>
                                            <applet>
			                                    <name>Posertrons_V2</name>
                                                <override>
                                                    <appletId>Posertron_Applet</appletId>
                                                    <register>AppletRegister_posertron.lua</register>
                                                    <params>
                                                        <model>
                                                            <mdl>rope.mdl</mdl>
                                                            <hkx>rope.hkx</hkx>
                                                        </model>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_block>
                                        <root_block_0>
			                                <rot type='vec'>0,0,0,0</rot>
                                            <scale type='vec'>1,1,1,1</scale>
                                            <pos type='vec'>-3.190,1.479,-20.632,0</pos>
                                            <applet>
			                                    <name>Posertrons_V2</name>
                                                <override>
                                                    <appletId>Posertron_Applet</appletId>
                                                    <register>AppletRegister_posertron.lua</register>
                                                    <params>
                                                        <model>
                                                            <mdl>rope.mdl</mdl>
                                                            <hkx>rope.hkx</hkx>
                                                        </model>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_block_0>
                                        <root_custom>
			                                <rot type='vec'>0,-208,0,-208</rot>
                                            <scale type='vec'>1,1,1,1</scale>
                                            <pos type='vec'>-2.596,1.790,16.674,0</pos>
                                            <applet>
			                                    <name>Posertrons_V2</name>
                                                <override>
                                                    <appletId>Posertron_Applet</appletId>
                                                    <register>AppletRegister_posertron.lua</register>
                                                    <params>
                                                        <model>
                                                            <mdl>customize_logo.mdl</mdl>
                                                        </model>
                                                    </params>
                                                </override>
		                                    </applet>
		                                </root_custom>
	                                </feature_root>
                                </lua>");
                        return;
                    default:
                        LoggerAccessor.LogWarn($"[PostAuthParameters] - group_def definition data was not found for group_def:{group_def}!");
                        break;
                }
                await ctx.Response.Send("<lua></lua>");
            });
        }
    }
}
