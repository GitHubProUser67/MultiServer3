using CustomLogger;
using System;
using System.IO;
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
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.Send();
                }
            });

            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/webassets/Sodium/{project}/{version}/HoloTip_Data.xml", async (ctx) =>
            {
                string? project = ctx.Request.Url.Parameters["project"];
                if (string.IsNullOrEmpty(project))
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.Send();
                    return;
                }
                switch (project)
                {
                    case "sodium_blimp":
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        ctx.Response.ContentType = "text/xml";
                        await ctx.Response.Send(@"<lua>
                    <SALES_04>
                        <radius type='num'>8.298</radius>
                        <dotFov type='num'>0.58</dotFov>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <textDataConditional>
                            <_>PURCHASE_ME_1</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <condition>
                                <_>AND</_>
                                <_>PLEB</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <Y_step type='num'>-222.6</Y_step>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <textData>
                            <_>PURCHASE_ME_3</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <Y_step type='num'>-222.6</Y_step>
                            <itterations type='num'>8.16</itterations>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <jitterBias type='num'>1</jitterBias>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <beamScale type='vec'>5.959,5.613,13.926,10.500</beamScale>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.60412</innerRadius>
                        <enableCondition>
                            <_>AND</_>
                            <_>PLEB</_>
                        </enableCondition>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <height type='num'>-0.9</height>
                        <look type='vec'>4.092,13.488,-12.977,0.000</look>
                        <tilt type='num'>359.2</tilt>
                        <pos type='vec'>5.448,13.029,-13.597,0.000</pos>
                    </SALES_04>
                    <PORTAL_JATOS_01>
                        <radius type='num'>8.298</radius>
                        <dotFov type='num'>0.74</dotFov>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <beamScale type='vec'>10.500,10.500,10.500,10.500</beamScale>
                        <enableCondition>
                            <_>AND</_>
                            <_>JATO</_>
                            <_>NOT</_>
                            <_>MINERALS</_>
                        </enableCondition>
                        <textData>
                            <_>PORTAL_HINT_1</_>
                            <_>PORTAL_HINT_2</_>
                            <Y_step type='num'>-222.6</Y_step>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterBias type='num'>1</jitterBias>
                            <itterations type='num'>8.16</itterations>
                            <X_size type='num'>3.8327</X_size>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <tilt type='num'>21.3</tilt>
                        <height type='num'>-0.9</height>
                        <textDataConditional>
                            <_>PORTAL_HINT_7</_>
                            <_>PORTAL_HINT_8</_>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <condition>
                                <_>AND</_>
                                <_>JATO</_>
                                <_>NOT</_>
                                <_>MINERALS</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <X_size type='num'>3.8327</X_size>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <Y_step type='num'>-222.6</Y_step>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <look type='vec'>-8.445,5.728,-12.525,0.000</look>
                        <pos type='vec'>-10.345,5.757,-11.706,0.000</pos>
                    </PORTAL_JATOS_01>
                    <PILOT_01>
                        <tilt type='num'>21.3</tilt>
                        <pos type='vec'>-0.042,13.277,3.755,0.000</pos>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <dotFov type='num'>0.74</dotFov>
                        <look type='vec'>-0.014,13.217,2.241,0.000</look>
                        <textData>
                            <_>PURCHASE_ME_1</_>
                            <Y_step type='num'>-222.6</Y_step>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterBias type='num'>1</jitterBias>
                            <itterations type='num'>8.16</itterations>
                            <X_size type='num'>3.8327</X_size>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <enableCondition>
                            <_>AND</_>
                            <_>PLEB</_>
                        </enableCondition>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <height type='num'>-0.9</height>
                        <textDataConditional>
                            <_>DENIED</_>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <condition>
                                <_>AND</_>
                                <_>PLEB</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <X_size type='num'>3.8327</X_size>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <Y_step type='num'>-222.6</Y_step>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <radius type='num'>8.298</radius>
                        <beamScale type='vec'>10.500,10.500,10.500,10.500</beamScale>
                    </PILOT_01>
                    <PORTAL_RES_01>
                        <radius type='num'>8.298</radius>
                        <beamScale type='vec'>10.500,10.500,10.500,10.500</beamScale>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <dotFov type='num'>0.74</dotFov>
                        <look type='vec'>-8.445,5.728,-12.525,0.000</look>
                        <height type='num'>-0.9</height>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <tilt type='num'>21.3</tilt>
                        <textData>
                            <_>PORTAL_HINT_1</_>
                            <_>PORTAL_HINT_2</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <Y_step type='num'>-222.6</Y_step>
                            <itterations type='num'>8.16</itterations>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <enableCondition>
                            <_>AND</_>
                            <_>NOT</_>
                            <_>JATO</_>
                            <_>MINERALS</_>
                        </enableCondition>
                        <textDataConditional>
                            <_>PORTAL_HINT_5</_>
                            <_>PORTAL_HINT_6</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <condition>
                                <_>AND</_>
                                <_>NOT</_>
                                <_>JATO</_>
                                <_>MINERALS</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <itterations type='num'>8.16</itterations>
                            <Y_step type='num'>-222.6</Y_step>
                            <jitterBias type='num'>1</jitterBias>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <pos type='vec'>-10.345,5.757,-11.706,0.000</pos>
                    </PORTAL_RES_01>
                    <PILOT_03>
                        <radius type='num'>8.298</radius>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <pos type='vec'>-1.039,3.324,16.767,0.000</pos>
                        <dotFov type='num'>-0.62</dotFov>
                        <enableCondition>
                            <_>AND</_>
                            <_>PLEB</_>
                        </enableCondition>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.461,0.000,0.194,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <height type='num'>1</height>
                        <tilt type='num'>357.6</tilt>
                        <textData>
                            <_>DENIED</_>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <Y_size type='num'>7.9428</Y_size>
                            <alignment>right</alignment>
                            <jitterHi type='num'>30.453</jitterHi>
                            <shadowColour type='vec'>0.801,0.433,0.000,0.311</shadowColour>
                            <itterations type='num'>8.16</itterations>
                            <X_size type='num'>7.4207</X_size>
                            <Y_step type='num'>-222.6</Y_step>
                            <colour type='vec'>0.960,0.864,0.000,1.000</colour>
                        </textData>
                        <look type='vec'>-0.992,4.036,15.758,0.000</look>
                        <beamSquash type='vec'>0.813,0.476,0.508,0.000</beamSquash>
                        <beamScale type='vec'>4.573,10.500,2.893,10.500</beamScale>
                    </PILOT_03>
                    <PORTAL_RES_02>
                        <radius type='num'>8.298</radius>
                        <dotFov type='num'>0.74</dotFov>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <beamScale type='vec'>10.500,10.500,10.500,10.500</beamScale>
                        <enableCondition>
                            <_>AND</_>
                            <_>NOT</_>
                            <_>JATO</_>
                            <_>MINERALS</_>
                        </enableCondition>
                        <textData>
                            <_>PORTAL_HINT_1</_>
                            <_>PORTAL_HINT_2</_>
                            <Y_step type='num'>-222.6</Y_step>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterBias type='num'>1</jitterBias>
                            <itterations type='num'>8.16</itterations>
                            <X_size type='num'>3.8327</X_size>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <tilt type='num'>21.3</tilt>
                        <height type='num'>-0.9</height>
                        <textDataConditional>
                            <_>PORTAL_HINT_5</_>
                            <_>PORTAL_HINT_6</_>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <condition>
                                <_>AND</_>
                                <_>NOT</_>
                                <_>JATO</_>
                                <_>MINERALS</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <X_size type='num'>3.8327</X_size>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <Y_step type='num'>-222.6</Y_step>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <look type='vec'>-3.653,5.432,-9.088,0.000</look>
                        <pos type='vec'>-4.149,5.755,-6.054,0.000</pos>
                    </PORTAL_RES_02>
                    <PORTAL_DEFAULT_02>
                        <radius type='num'>8.298</radius>
                        <dotFov type='num'>0.74</dotFov>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <beamScale type='vec'>10.500,10.500,10.500,10.500</beamScale>
                        <enableCondition>
                            <_>AND</_>
                            <_>NOT</_>
                            <_>JATO</_>
                            <_>NOT</_>
                            <_>MINERALS</_>
                        </enableCondition>
                        <textData>
                            <_>PORTAL_HINT_1</_>
                            <_>PORTAL_HINT_2</_>
                            <Y_step type='num'>-222.6</Y_step>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterBias type='num'>1</jitterBias>
                            <itterations type='num'>8.16</itterations>
                            <X_size type='num'>3.8327</X_size>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <tilt type='num'>21.3</tilt>
                        <height type='num'>-0.9</height>
                        <textDataConditional>
                            <_>PORTAL_HINT_1</_>
                            <_>PORTAL_HINT_4</_>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <condition>
                                <_>AND</_>
                                <_>NOT</_>
                                <_>JATO</_>
                                <_>NOT</_>
                                <_>MINERALS</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <X_size type='num'>3.8327</X_size>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <Y_step type='num'>-222.6</Y_step>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <look type='vec'>-2.519,5.615,-8.572,0.000</look>
                        <pos type='vec'>-4.149,5.755,-6.054,0.000</pos>
                    </PORTAL_DEFAULT_02>
                    <BINOCULAR_01>
                        <radius type='num'>8.298</radius>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <pos type='vec'>-10.239,13.029,-9.983,0.000</pos>
                        <beamScale type='vec'>5.959,5.613,13.926,10.500</beamScale>
                        <look type='vec'>-8.837,13.432,-11.384,0.000</look>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <textData>
                            <_>BINOC_HINT</_>
                            <Y_step type='num'>-222.6</Y_step>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <X_size type='num'>3.8327</X_size>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <tilt type='num'>359.2</tilt>
                        <height type='num'>-0.9</height>
                        <enableCondition>
                            <_>AND</_>
                            <_>ALWAYS</_>
                        </enableCondition>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <dotFov type='num'>0.74</dotFov>
                    </BINOCULAR_01>
                    <PORTAL_JATOS_02>
                        <tilt type='num'>21.3</tilt>
                        <beamScale type='vec'>10.500,10.500,10.500,10.500</beamScale>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <enableCondition>
                            <_>AND</_>
                            <_>JATO</_>
                            <_>NOT</_>
                            <_>MINERALS</_>
                        </enableCondition>
                        <height type='num'>-0.9</height>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <radius type='num'>8.298</radius>
                        <dotFov type='num'>0.74</dotFov>
                        <textData>
                            <_>PORTAL_HINT_1</_>
                            <_>PORTAL_HINT_2</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <Y_step type='num'>-222.6</Y_step>
                            <itterations type='num'>8.16</itterations>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <textDataConditional>
                            <_>PORTAL_HINT_7</_>
                            <_>PORTAL_HINT_8</_>
                            <Y_step type='num'>-222.6</Y_step>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <condition>
                                <_>AND</_>
                                <_>JATO</_>
                                <_>NOT</_>
                                <_>MINERALS</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <X_size type='num'>3.8327</X_size>
                            <itterations type='num'>8.16</itterations>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <jitterBias type='num'>1</jitterBias>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <look type='vec'>-3.049,5.616,-8.660,0.000</look>
                        <pos type='vec'>-4.149,5.755,-6.054,0.000</pos>
                    </PORTAL_JATOS_02>
                    <BINOCULAR_04>
                        <radius type='num'>8.298</radius>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <pos type='vec'>-6.553,8.176,-34.355,0.000</pos>
                        <beamScale type='vec'>5.959,5.613,13.926,10.500</beamScale>
                        <look type='vec'>-4.821,8.477,-33.990,0.000</look>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <textData>
                            <_>BINOC_HINT</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <itterations type='num'>8.16</itterations>
                            <Y_step type='num'>-222.6</Y_step>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <tilt type='num'>359.2</tilt>
                        <height type='num'>-0.9</height>
                        <enableCondition>
                            <_>AND</_>
                            <_>ALWAYS</_>
                        </enableCondition>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <dotFov type='num'>0.74</dotFov>
                    </BINOCULAR_04>
                    <BINOCULAR_02>
                        <radius type='num'>8.298</radius>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <pos type='vec'>7.616,13.030,1.513,0.000</pos>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <look type='vec'>6.506,13.268,0.230,0.000</look>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <textData>
                            <_>BINOC_HINT</_>
                            <Y_step type='num'>-222.6</Y_step>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <X_size type='num'>3.8327</X_size>
                            <itterations type='num'>8.16</itterations>
                            <jitterBias type='num'>1</jitterBias>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <beamScale type='vec'>5.959,5.613,13.926,10.500</beamScale>
                        <height type='num'>-0.9</height>
                        <enableCondition>
                            <_>AND</_>
                            <_>ALWAYS</_>
                        </enableCondition>
                        <tilt type='num'>359.2</tilt>
                        <dotFov type='num'>0.74</dotFov>
                    </BINOCULAR_02>
                    <TELEPORTER_PARKED>
                        <tilt type='num'>359.2</tilt>
                        <pos type='vec'>6.507,13.024,3.995,0.000</pos>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <look type='vec'>4.803,13.495,2.517,0.000</look>
                        <enableCondition>
                            <_>AND</_>
                            <_>TELEPORTER</_>
                        </enableCondition>
                        <height type='num'>-0.9</height>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <beamScale type='vec'>5.959,5.613,13.926,10.500</beamScale>
                        <radius type='num'>8.298</radius>
                        <textData>
                            <_>TELEPORTER_HINT_3</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <Y_step type='num'>-222.6</Y_step>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <textDataConditional>
                            <_>TELEPORTER_HINT_4</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <condition>
                                <_>AND</_>
                                <_>TELEPORTER</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <Y_step type='num'>-222.6</Y_step>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <dotFov type='num'>0.74</dotFov>
                    </TELEPORTER_PARKED>
                    <BINOCULAR_03>
                        <radius type='num'>8.298</radius>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <pos type='vec'>-6.452,5.755,-17.727,0.000</pos>
                        <tilt type='num'>359.2</tilt>
                        <look type='vec'>-5.461,6.177,-16.880,0.000</look>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <textData>
                            <_>BINOC_HINT</_>
                            <Y_step type='num'>-222.6</Y_step>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <X_size type='num'>3.8327</X_size>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <height type='num'>-0.9</height>
                        <enableCondition>
                            <_>AND</_>
                            <_>ALWAYS</_>
                        </enableCondition>
                        <dotFov type='num'>0.74</dotFov>
                        <beamScale type='vec'>5.959,5.613,13.926,10.500</beamScale>
                    </BINOCULAR_03>
                    <BINOCULAR_05>
                        <radius type='num'>8.298</radius>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <pos type='vec'>6.626,8.163,-33.107,0.000</pos>
                        <beamScale type='vec'>5.959,5.613,13.926,10.500</beamScale>
                        <look type='vec'>5.315,8.449,-33.745,0.000</look>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <textData>
                            <_>BINOC_HINT</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <itterations type='num'>8.16</itterations>
                            <Y_step type='num'>-222.6</Y_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <tilt type='num'>359.2</tilt>
                        <height type='num'>-0.9</height>
                        <enableCondition>
                            <_>AND</_>
                            <_>ALWAYS</_>
                        </enableCondition>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <dotFov type='num'>0.74</dotFov>
                    </BINOCULAR_05>
                    <INFO_POINT>
                        <radius type='num'>8.298</radius>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <pos type='vec'>-7.475,13.042,3.772,0.000</pos>
                        <tilt type='num'>359.2</tilt>
                        <look type='vec'>-7.204,13.134,3.621,0.000</look>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <textData>
                            <_>INFO_HINT_1</_>
                            <Y_step type='num'>-222.6</Y_step>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <X_size type='num'>3.8327</X_size>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <height type='num'>-0.9</height>
                        <enableCondition>
                            <_>AND</_>
                            <_>ALWAYS</_>
                        </enableCondition>
                        <dotFov type='num'>0.74</dotFov>
                        <beamScale type='vec'>5.959,5.613,13.926,10.500</beamScale>
                    </INFO_POINT>
                    <PILOT_02>
                        <radius type='num'>8.298</radius>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <pos type='vec'>0.960,3.328,16.767,0.000</pos>
                        <beamScale type='vec'>4.573,10.500,2.893,10.500</beamScale>
                        <enableCondition>
                            <_>AND</_>
                            <_>PLEB</_>
                        </enableCondition>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.461,0.000,0.194,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <height type='num'>1</height>
                        <tilt type='num'>357.6</tilt>
                        <textData>
                            <_>DENIED</_>
                            <X_size type='num'>7.4207</X_size>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <Y_size type='num'>7.9428</Y_size>
                            <jitterHi type='num'>30.453</jitterHi>
                            <alignment>left</alignment>
                            <shadowColour type='vec'>0.801,0.433,0.000,0.311</shadowColour>
                            <itterations type='num'>8.16</itterations>
                            <Y_step type='num'>-222.6</Y_step>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <colour type='vec'>0.960,0.864,0.000,1.000</colour>
                        </textData>
                        <look type='vec'>0.906,4.060,15.608,0.000</look>
                        <beamSquash type='vec'>0.813,0.476,0.508,0.000</beamSquash>
                        <dotFov type='num'>-0.78</dotFov>
                    </PILOT_02>
                    <SALES_01>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <dotFov type='num'>0.74</dotFov>
                        <textDataConditional>
                            <_>PURCHASE_ME_1</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <condition>
                                <_>AND</_>
                                <_>PLEB</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <Y_step type='num'>-222.6</Y_step>
                            <jitterHi type='num'>30.453</jitterHi>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <itterations type='num'>8.16</itterations>
                            <jitterBias type='num'>1</jitterBias>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <radius type='num'>8.298</radius>
                        <tilt type='num'>359.2</tilt>
                        <textData>
                            <_>PURCHASE_ME_2</_>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <maxWidth type='num'>1280</maxWidth>
                            <Y_step type='num'>-222.6</Y_step>
                            <jitterHi type='num'>30.453</jitterHi>
                            <X_size type='num'>3.8327</X_size>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <pos type='vec'>-7.478,13.037,-2.730,0.000</pos>
                        <look type='vec'>-5.906,13.493,-3.382,0.000</look>
                        <height type='num'>-0.9</height>
                        <enableCondition>
                            <_>AND</_>
                            <_>PLEB</_>
                        </enableCondition>
                        <beamScale type='vec'>5.959,5.613,13.926,10.500</beamScale>
                    </SALES_01>
                    <PORTAL_DEFAULT_01>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <beamScale type='vec'>10.500,10.500,10.500,10.500</beamScale>
                        <enableCondition>
                            <_>AND</_>
                            <_>NOT</_>
                            <_>JATO</_>
                            <_>NOT</_>
                            <_>MINERALS</_>
                        </enableCondition>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <radius type='num'>8.298</radius>
                        <tilt type='num'>21.3</tilt>
                        <dotFov type='num'>0.74</dotFov>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <look type='vec'>-8.430,5.514,-12.581,0.000</look>
                        <textData>
                            <_>PORTAL_HINT_1</_>
                            <_>PORTAL_HINT_2</_>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <maxWidth type='num'>1280</maxWidth>
                            <X_size type='num'>3.8327</X_size>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterBias type='num'>1</jitterBias>
                            <itterations type='num'>8.16</itterations>
                            <Y_step type='num'>-222.6</Y_step>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <height type='num'>-0.9</height>
                        <textDataConditional>
                            <_>PORTAL_HINT_1</_>
                            <_>PORTAL_HINT_3</_>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <condition>
                                <_>AND</_>
                                <_>NOT</_>
                                <_>JATO</_>
                                <_>NOT</_>
                                <_>MINERALS</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <X_size type='num'>3.8327</X_size>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterBias type='num'>1</jitterBias>
                            <itterations type='num'>8.16</itterations>
                            <Y_step type='num'>-222.6</Y_step>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <pos type='vec'>-10.345,5.757,-11.706,0.000</pos>
                    </PORTAL_DEFAULT_01>
                    <TELEPORTER_MOVING>
                        <radius type='num'>8.298</radius>
                        <pos type='vec'>6.507,13.024,3.995,0.000</pos>
                        <scale type='vec'>0.457,0.450,0.247,1.000</scale>
                        <tilt type='num'>359.2</tilt>
                        <look type='vec'>4.803,13.495,2.517,0.000</look>
                        <height type='num'>-0.9</height>
                        <models>
                            <tintLevel type='num'>1.2975</tintLevel>
                            <beam>HoloBeam</beam>
                            <tint type='vec'>0.000,0.046,0.461,1.000</tint>
                            <screen>Holo_Screen1</screen>
                            <unit>HoloUnit</unit>
                        </models>
                        <innerRadius type='num'>0.98264</innerRadius>
                        <enableCondition>
                            <_>AND</_>
                            <_>NOT</_>
                            <_>TELEPORTER</_>
                        </enableCondition>
                        <beamSquash type='vec'>0.877,0.294,0.465,0.000</beamSquash>
                        <textData>
                            <_>TELEPORTER_HINT_1</_>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <X_step type='num'>0</X_step>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <X_size type='num'>3.8327</X_size>
                            <itterations type='num'>8.16</itterations>
                            <jitterBias type='num'>1</jitterBias>
                            <Y_step type='num'>-222.6</Y_step>
                            <Y_size type='num'>10.574</Y_size>
                        </textData>
                        <textDataConditional>
                            <_>TELEPORTER_HINT_2</_>
                            <X_size type='num'>3.8327</X_size>
                            <X_step type='num'>0</X_step>
                            <jitterBias type='num'>1</jitterBias>
                            <condition>
                                <_>AND</_>
                                <_>NOT</_>
                                <_>TELEPORTER</_>
                                <_>Timer_10sec</_>
                            </condition>
                            <maxWidth type='num'>1280</maxWidth>
                            <colour type='vec'>0.000,0.096,0.960,1.000</colour>
                            <jitterHi type='num'>30.453</jitterHi>
                            <jitterLow type='num'>1.8545</jitterLow>
                            <itterations type='num'>8.16</itterations>
                            <shadowColour type='vec'>0.000,0.320,0.801,0.311</shadowColour>
                            <Y_step type='num'>-222.6</Y_step>
                            <Y_size type='num'>10.574</Y_size>
                        </textDataConditional>
                        <dotFov type='num'>0.74</dotFov>
                        <beamScale type='vec'>5.959,5.613,13.926,10.500</beamScale>
                    </TELEPORTER_MOVING>
                </lua>");
                        return;
                    default:
                        string xmlPath = $"/webassets/Sodium/{project}/{ctx.Request.Url.Parameters["version"]}/HoloTip_Data.xml";
                        string filePath = !ApacheNetServerConfiguration.DomainFolder ? ApacheNetServerConfiguration.HTTPStaticFolder + xmlPath : ApacheNetServerConfiguration.HTTPStaticFolder + $"/{ctx.Request.RetrieveHeaderValue("Host")}" + xmlPath;

                        LoggerAccessor.LogWarn($"[PostAuthParameters] - HoloTip_Data data was not found for project:{project}, falling back to static file.");

                        if (File.Exists(filePath))
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "text/xml";
                            await ctx.Response.Send(File.ReadAllText(filePath));
                            return;
                        }
                        break;
                }
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await ctx.Response.Send();
            });

            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/webassets/Sodium/{project}/{version}/{region}.xml", async (ctx) =>
            {
                string? project = ctx.Request.Url.Parameters["project"];
                if (string.IsNullOrEmpty(project))
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.Send();
                    return;
                }
                switch (project)
                {
                    case "sodium_blimp":
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        ctx.Response.ContentType = "text/xml";
                        await ctx.Response.Send("");
                        return;
                    default:
                        string xmlPath = $"/webassets/Sodium/{project}/{ctx.Request.Url.Parameters["version"]}/{ctx.Request.Url.Parameters["region"]}.xml";
                        string filePath = !ApacheNetServerConfiguration.DomainFolder ? ApacheNetServerConfiguration.HTTPStaticFolder + xmlPath : ApacheNetServerConfiguration.HTTPStaticFolder + $"/{ctx.Request.RetrieveHeaderValue("Host")}" + xmlPath;

                        LoggerAccessor.LogWarn($"[PostAuthParameters] - regionalized data was not found for project:{project}, falling back to static file.");

                        if (File.Exists(filePath))
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "text/xml";
                            await ctx.Response.Send(File.ReadAllText(filePath));
                            return;
                        }
                        break;
                }
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await ctx.Response.Send();
            });

            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/static/SodiumBlimp/{version}/defs/{defs}.xml", async (ctx) =>
            {
                string? defs = ctx.Request.Url.Parameters["defs"];
                if (string.IsNullOrEmpty(defs))
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.Send();
                    return;
                }
                switch (defs)
                {
                    case "craft_defs.xml":
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        ctx.Response.ContentType = "text/xml";
                        await ctx.Response.Send(WebAPIService.OHS.LUA2XmlProcessor.TransformLuaTableToXml(@"
                    local TableFromInput = {
                        [""defs""] = { [""silver""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/silver.dds""
                        },
                        [""crimson""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/crimson.dds""
                        },
                        [""classic_green""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/classic_green.dds""
                        },
                        [""bumble_bee""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/bumble_bee.dds""
                        },
                        [""salamander""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/salamander.dds""
                        },
                        [""sonic_camo""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/sonic_camo.dds""
                        },
                        [""phoenix""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/phoenix.dds""
                        },
                        [""marshmallow""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/marshmallow.dds""
                        },
                        [""velocity_racer_green""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/velocity_racer_green.dds""
                        },
                        [""animal_magnetism""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/animal_magnetism.dds""
                        },
                        [""fruit_salad""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/fruit_salad.dds""
                        },
                        [""sodium_one_chili_red_edition""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/sodium_one_chili_red_edition.dds""
                        },
                        [""blue_viper""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/blue_viper.dds""
                        },
                        [""apple_twist""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/apple_twist.dds""
                        },
                        [""union_jackson""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/union_jackson.dds""
                        },
                        [""hiromaru_racer""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/hiromaru_racer.dds""
                        },
                        [""star_spangled""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/star_spangled.dds""
                        },
                        [""seasons_greetings""] = {
                            [""skin_texture""] = ""webassets/Sodium/sodium_blimp/craft_data/textures/seasons_greetings.dds""
                        } }
                    }

                    return Encode(TableFromInput, 4, 4)
                    ").Trim());
                        return;
                    default:
                        string xmlPath = $"/static/SodiumBlimp/{ctx.Request.Url.Parameters["version"]}/defs/{defs}.xml";
                        string filePath = !ApacheNetServerConfiguration.DomainFolder ? ApacheNetServerConfiguration.HTTPStaticFolder + xmlPath : ApacheNetServerConfiguration.HTTPStaticFolder + $"/{ctx.Request.RetrieveHeaderValue("Host")}" + xmlPath;

                        LoggerAccessor.LogWarn($"[PostAuthParameters] - definition data was not found for defs:{defs}, falling back to static file.");

                        if (File.Exists(filePath))
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "text/xml";
                            await ctx.Response.Send(File.ReadAllText(filePath));
                            return;
                        }
                        break;
                }
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await ctx.Response.Send();
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

                    return XmlConvert.LuaToXml(TableFromInput, 'lua', 1)
                    "));
            });

            server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/static/Lockwood/Features/Venue/{scenetype}/{build}/{country}/camPath.xml", async (ctx) =>
            {
                // For now this returns a default table, TODO: feed this with actual data.
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                ctx.Response.ContentType = "text/xml";
                await ctx.Response.Send("<lua></lua>");
            });
            #endregion
        }
    }
}
