export default class CloudApi {
    async init() {
        this.appSettings = await this.getConfig();
        this.token = await this.getToken();
    }

    callApi(endpoint, method, args) {
        return new Promise((resolve, reject) => {
            let body, formData = null;
            let headers = { "Authorization": "Bearer " + this.token, 'Accept': '*/*' };

            if (args) {
                if (args.query) {
                    endpoint += `?${args.query}`;
                }
                else if (args.id) {
                    endpoint += `/${args.id}`;
                }

                if (args.body) {
                    body = args.body;
                    headers = { 'Content-Type': 'application/json', ...headers };
                }
                else if (args.formData) {
                    formData = args.formData;
                }
            }

            fetch(
                endpoint,
                {
                    headers: headers,
                    method,
                    body: !formData ? body : formData
                })
                .then(response => {
                    if (response.status === 401 || response.status === 403) {
                        return reject(response.statusText);
                    }

                    return resolve(response);
                })
                .catch(error => {
                    return reject(error);
                });
        });
    }

    async getToken() {
        const response = await this.callApi('Auth/GetToken', 'GET');
        return await response.text();
    }

    async getConfig() {
        const response = await this.callApi('Auth/GetConfig', 'GET');
        const jsonResponse = await response.json();
        return { ...jsonResponse, "baseUrl": `https://${jsonResponse.armEndpoint}/subscriptions/${jsonResponse.subscription}/resourceGroups/${jsonResponse.resourceGroup}/providers/Microsoft.Media/videoAnalyzers` };
    }

    async getVideoAnalyzers() {
        try {
            const { baseUrl, apiVersion } = this.appSettings;
            const url = `${baseUrl}${apiVersion}`;
            const response = await this.callApi(url, 'GET');
            const jsonResponse = await response.json();

            if (response.ok) {
                return jsonResponse.value;
            }
            else {
                throw new Error(jsonResponse.error.message);
            }
        } catch (err) {
            throw err;
        }
    }

    //async deleteLivePipelineOperation(livePipeline) {
    //    const { baseUrl, apiVersion, accountName } = this.appSettings;
    //    const token = this.token;
    //    const url = `${baseUrl}/${accountName}/livePipelines/${livePipeline}${apiVersion}`;

    //    return this.callApi('Opportunity', 'DELETE');
    //    try {
    //        const response = await fetch(url, {
    //            method: 'DELETE',
    //            headers: {
    //                "Authorization": `Bearer ${token}`
    //            }
    //        });

    //        if (response.ok) {
    //            await this.getLivePipelines();
    //        }
    //        else {
    //            const errorMessageObj = await response.json();
    //            alert(`Cannot delete livepipeline: ${errorMessageObj.error.message}`);
    //        }
    //    }
    //    catch (e) {
    //        console.log(e);
    //    }
    //}

    //async deleteVideoOperation(videoName) {
    //    const { baseUrl, apiVersion, accountName } = this.state.appSettings;
    //    const token = this.token;
    //    const url = `${baseUrl}/${accountName}/videos/${videoName}${apiVersion}`;
    //    try {
    //        const response = await fetch(url, {
    //            method: 'DELETE',
    //            headers: {
    //                "Authorization": `Bearer ${token}`
    //            }
    //        });

    //        if (!response.ok) {
    //            const errorMessageObj = await response.json();
    //            console.log(`Cannot delete video ${videoName}: ${errorMessageObj.error.message}`);
    //        }
    //    }
    //    catch (e) {
    //        console.log(e);
    //    }
    //}

    //async deletePipelineTopologyOperation(pipelineTopologyName) {
    //    const { baseUrl, apiVersion, accountName } = this.state.appSettings;
    //    const token = this.token;
    //    const url = `${baseUrl}/${accountName}/pipelineTopologies/${pipelineTopologyName}${apiVersion}`;
    //    try {
    //        const response = await fetch(url, {
    //            method: 'DELETE',
    //            headers: {
    //                "Authorization": `Bearer ${token}`
    //            }
    //        });

    //        if (response.ok) {
    //            await this.getPipelinesTopologies();
    //        }
    //        else {
    //            const errorMessageObj = await response.json();
    //            alert(`Cannot delete pipelineTopology: ${errorMessageObj.error.message}`);
    //        }
    //    }
    //    catch (e) {
    //        console.log(e);
    //    }
    //}

    //async createPipelineTopologyOperation(event) {
    //    event.preventDefault();
    //    const { pipelineTopologyName, behindProxy } = this.state;
    //    const { baseUrl, apiVersion, accountName, ioTHubDeviceId, ioTHubArmId, ioTHubUserAssignedManagedIdentityArmId } = this.state.appSettings;

    //    let body = {
    //        "Name": pipelineTopologyName,
    //        "Kind": "liveUltraLowLatency",
    //        "Sku": {
    //            "Name": "S1",
    //            "Tier": "Standard"
    //        },
    //        "Properties": {
    //            "description": "pipeline topology test description",
    //            "parameters": [
    //                {
    //                    "name": "rtspUrlParameter",
    //                    "type": "String",
    //                    "description": "rtsp source url parameter"
    //                },
    //                {
    //                    "name": "rtspUsernameParameter",
    //                    "type": "String",
    //                    "description": "rtsp source username parameter"
    //                },
    //                {
    //                    "name": "rtspPasswordParameter",
    //                    "type": "SecretString",
    //                    "description": "rtsp source password parameter"
    //                },
    //                {
    //                    "name": "videoNameParameter",
    //                    "type": "String",
    //                    "description": "video name parameter"
    //                }
    //            ],
    //            "sources": [
    //                {
    //                    "@type": "#Microsoft.VideoAnalyzer.RtspSource",
    //                    "name": "rtspSource",
    //                    "transport": "tcp",
    //                    "endpoint": {
    //                        "@type": "#Microsoft.VideoAnalyzer.UnsecuredEndpoint",
    //                        "url": "${rtspUrlParameter}",
    //                        "credentials": {
    //                            "@type": "#Microsoft.VideoAnalyzer.UsernamePasswordCredentials",
    //                            "username": "${rtspUsernameParameter}",
    //                            "password": "${rtspPasswordParameter}"
    //                        }
    //                    }
    //                }
    //            ],
    //            "sinks": [
    //                {
    //                    "@type": "#Microsoft.VideoAnalyzer.VideoSink",
    //                    "name": "videoSink",
    //                    "videoName": "${videoNameParameter}",
    //                    "videoCreationProperties": {
    //                        "title": "Sample Video",
    //                        "description": "Sample Video",
    //                        "segmentLength": "PT30S"
    //                    },
    //                    "inputs": [
    //                        {
    //                            "nodeName": "rtspSource"
    //                        }
    //                    ]
    //                }
    //            ]
    //        }
    //    };

    //    if (behindProxy) {
    //        let parameters = body.Properties.parameters;
    //        const deviceIdParam = {
    //            "name": RtspDeviceIdParameter,
    //            "type": "String",
    //            "description": "device id parameter"
    //        }
    //        parameters.push(deviceIdParam);

    //        let source = body.Properties.sources.pop();
    //        let endpoint = source.endpoint;
    //        source.endpoint = {
    //            ...endpoint, "tunnel": {
    //                "@type": "#Microsoft.VideoAnalyzer.IotSecureDeviceRemoteTunnel",
    //                "deviceId": "${" + RtspDeviceIdParameter + "}",
    //                "iotHubArmId": ioTHubArmId,
    //                "userAssignedManagedIdentityArmId": ioTHubUserAssignedManagedIdentityArmId
    //            }
    //        };

    //        body.Properties.sources.push(source);
    //    }

    //    const token = this.token;
    //    const url = `${baseUrl}/${accountName}/pipelineTopologies/${pipelineTopologyName}${apiVersion}`;
    //    try {
    //        const response = await fetch(url, {
    //            method: 'PUT',
    //            headers: {
    //                "Authorization": `Bearer ${token}`,
    //                "Content-Type": "application/json"
    //            },
    //            body: JSON.stringify(body)
    //        });

    //        if (response.ok) {
    //            this.setState({ pipelineTopologyName: "", videoName: "", behindProxy: false }, async () =>
    //                await this.getPipelinesTopologies());
    //        }
    //        else {
    //            const errorMessageObj = await response.json();
    //            alert(`Cannot create tje pipelineTopology: ${errorMessageObj.error.message}`);
    //        }
    //    }
    //    catch (e) {
    //        console.log(e);
    //    }
    //    finally {
    //        this.setState({ loadingPipelineTopologies: false });
    //    }
    //}

    //async createLivePipelineOperation(event) {
    //    event.preventDefault();
    //    const { livePipelineName, rtspUrl, rtspUsername, rtspPassword, livePipelineTopologyName, videoName, deviceId, showDeviceId } = this.state;
    //    const { baseUrl, accountName, apiVersion } = this.state.appSettings;

    //    let body = {
    //        "name": livePipelineName,
    //        "properties": {
    //            "topologyName": livePipelineTopologyName,
    //            "description": "live pipeline test description",
    //            "bitrateKbps": 500,
    //            "parameters": [
    //                {
    //                    "name": "rtspUrlParameter",
    //                    "value": rtspUrl
    //                },
    //                {
    //                    "name": "rtspUsernameParameter",
    //                    "value": rtspUsername
    //                },
    //                {
    //                    "name": "rtspPasswordParameter",
    //                    "value": rtspPassword
    //                },
    //                {
    //                    "name": "videoNameParameter",
    //                    "value": videoName
    //                }
    //            ]
    //        }
    //    }

    //    if (showDeviceId && deviceId.length > 0) {
    //        const deviceParam = {
    //            "name": RtspDeviceIdParameter,
    //            "value": deviceId
    //        };

    //        body.properties.parameters.push(deviceParam);
    //    }

    //    const token = this.token;
    //    const url = `${baseUrl}/${accountName}/livePipelines/${livePipelineName}${apiVersion}`;
    //    try {
    //        const response = await fetch(url, {
    //            method: 'PUT',
    //            headers: {
    //                "Authorization": `Bearer ${token}`,
    //                "Content-Type": "application/json"
    //            },
    //            body: JSON.stringify(body)
    //        });

    //        if (response.ok) {
    //            this.setState({ livePipelineName: "", rtspUrl: "", rtspUsername: "", rtspPassword: "", livePipelineTopologyName: "", videoName: "" },
    //                async () => await this.getLivePipelines());
    //        }
    //        else {
    //            const errorMessageObj = await response.json();
    //            alert(`Cannot create livepipeline: ${errorMessageObj.error.message}`);
    //        }
    //    }
    //    catch (e) {
    //        console.log(e);
    //    }
    //    finally {
    //        this.setState({ loadingLivePipelines: false });
    //    }
    //}

    async checkStatus(asyncOpUrl) {
        try {
            const asyncResponse = await this.callApi(asyncOpUrl, 'GET');

            if (asyncResponse.ok) {
                const jsonResp = JSON.parse(await asyncResponse.text());
                if (jsonResp.status === "Running") {
                    return await this.checkStatus(asyncOpUrl);
                } else if (jsonResp.status === "Succeeded") {
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                throw new Error(await asyncResponse.text());
            }
        }
        catch (e) {
            throw new Error(e);
        }
    }

    async changeStateLivePipeline(livePipeline, properties) {
        try {
            const { baseUrl, accountName, apiVersion } = this.appSettings;
            const action = properties.state.toUpperCase() === "INACTIVE" ? "activate" : "deactivate";

            const url = `${baseUrl}/${accountName}/livePipelines/${livePipeline}/${action}${apiVersion}`;
            const response = await this.callApi(url, 'POST');

            if (response.ok) {
                let asyncOpUrl = response.headers.get("azure-asyncoperation");
                const asyncResponse = await this.callApi(asyncOpUrl, 'GET');

                if (asyncResponse.ok) {
                    let result = await this.checkStatus(asyncOpUrl);

                    if (result) {
                        return;
                    }
                    else {
                        const errorMessage = "Operation failed, please check the console log."
                        throw new Error(errorMessage);
                    }
                }
                else {
                    const errorMessage = await response.text();
                    throw new Error(errorMessage);
                }
            }
            else {
                const jsonResponse = await response.json();
                throw new Error(jsonResponse.error.message);
            }
        } catch (err) {
            throw err;
        }
    }

    async getPipelinesTopologies() {
        try {
            const { baseUrl, accountName, apiVersion } = this.appSettings;
            const url = `${baseUrl}/${accountName}/pipelineTopologies${apiVersion}`;
            const response = await this.callApi(url, 'GET');
            const jsonResponse = await response.json();

            if (response.ok) {
                return jsonResponse.value;
            }
            else {
                throw new Error(jsonResponse.error.message);
            }
        } catch (err) {
            throw err;
        }
    }

    async getLivePipelines() {
        try {
            const { baseUrl, accountName, apiVersion } = this.appSettings;
            const url = `${baseUrl}/${accountName}/livePipelines${apiVersion}`;
            const response = await this.callApi(url, 'GET');
            const jsonResponse = await response.json();

            if (response.ok) {
                return jsonResponse.value;
            }
            else {
                throw new Error(jsonResponse.error.message);
            }
        } catch (err) {
            throw err;
        }
    }

    //async getVideoPlayback(videoName, pipelineName) {
    //    const { baseUrl, accountName, apiVersion } = this.state.appSettings;
    //    const token = this.token;
    //    const url = `${baseUrl}/${accountName}/videos/${videoName}${apiVersion}`;
    //    const authUrl = `${baseUrl}/${accountName}/videos/${videoName}/listStreamingToken${apiVersion}`;
    //    try {
    //        const response = await fetch(url, {
    //            method: 'GET',
    //            headers: {
    //                "Authorization": `Bearer ${token}`
    //            }
    //        });

    //        let tunneledRtspUrl = "";
    //        let playbackToken = "";
    //        if (response.ok) {
    //            const jsonResponse = await response.json();
    //            tunneledRtspUrl = jsonResponse.properties.streaming.rtspTunnelUrl;

    //            const responseAuth = await fetch(authUrl, {
    //                method: 'POST',
    //                headers: {
    //                    "Authorization": `Bearer ${token}`
    //                }
    //            });

    //            if (responseAuth.ok) {
    //                const jsonAuthResponse = await responseAuth.json();
    //                playbackToken = jsonAuthResponse.token;
    //            }
    //            else {
    //                const errorMessageObj = await responseAuth.json();
    //                alert(`Cannot get video playback token: ${errorMessageObj.error.message}`);
    //            }
    //        }
    //        else {
    //            const errorMessageObj = await response.json();
    //            alert(`Cannot get video playback url: ${errorMessageObj.error.message}`);
    //        }

    //        this.renderVideoPlayer(tunneledRtspUrl, playbackToken, pipelineName);
    //    }
    //    catch (e) {
    //        console.log(e);
    //    }
    //    finally {
    //        this.setState({ loadingLivePipelines: false });
    //    }
    //}
}

