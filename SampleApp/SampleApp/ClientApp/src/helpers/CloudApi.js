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

    async deleteLivePipeline(livePipeline) {
        try {
            const { baseUrl, apiVersion, accountName } = this.appSettings;
            const url = `${baseUrl}/${accountName}/livePipelines/${livePipeline}${apiVersion}`;
            const response = await this.callApi(url, 'DELETE');

            if (!response.ok) {
                const errorMessageObj = await response.json();
                throw new Error(errorMessageObj.error.message);
            }
        }
        catch (e) {
            throw e;
        }
    }

    async deleteVideo(videoName) {
        try {
            const { baseUrl, apiVersion, accountName } = this.appSettings;
            const url = `${baseUrl}/${accountName}/videos/${videoName}${apiVersion}`;
            const response = await this.callApi(url, 'DELETE');

            if (!response.ok) {
                const errorMessageObj = await response.json();
                throw new Error(errorMessageObj.error.message);
            }
        }
        catch (e) {
            throw e;
        }
    }

    async deletePipelineTopology(pipelineTopologyName) {
        try {
            const { baseUrl, apiVersion, accountName } = this.appSettings;
            const url = `${baseUrl}/${accountName}/pipelineTopologies/${pipelineTopologyName}${apiVersion}`;
            const response = await this.callApi(url, 'DELETE');

            if (!response.ok) {
                const errorMessageObj = await response.json();
                throw new Error(errorMessageObj.error.message);
            }
        }
        catch (e) {
            throw e;
        }
    }

    async createPipelineTopology(pipelineTopology) {
        try {
            const { baseUrl, accountName, apiVersion } = this.appSettings;
            const url = `${baseUrl}/${accountName}/pipelineTopologies/${pipelineTopology.Name}${apiVersion}`;
            const response = await this.callApi(url, 'PUT', { body: JSON.stringify(pipelineTopology) });

            if (!response.ok) {
                const errorMessageObj = await response.json();
                throw new Error(errorMessageObj.error.message);
            }
        }
        catch (e) {
            throw e;
        }
    }

    async createLivePipeline(livePipeline) {
        try {
            const { baseUrl, accountName, apiVersion } = this.appSettings;
            const url = `${baseUrl}/${accountName}/livePipelines/${livePipeline.name}${apiVersion}`;
            const response = await this.callApi(url, 'PUT', { body: JSON.stringify(livePipeline) });

            if (!response.ok) {
                const errorMessageObj = await response.json();
                throw new Error(errorMessageObj.error.message);
            }
        }
        catch (e) {
            throw e;
        }
    }

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
                const errorMessageObj = await asyncResponse.json();
                throw new Error(errorMessageObj.error.message);
            }
        }
        catch (e) {
            throw e;
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

    async getVideoPlayback(videoName) {
        try {
            const { baseUrl, accountName, apiVersion } = this.appSettings;
            const url = `${baseUrl}/${accountName}/videos/${videoName}${apiVersion}`;
            const authUrl = `${baseUrl}/${accountName}/videos/${videoName}/listStreamingToken${apiVersion}`;

            const response = await this.callApi(url, 'GET');
            let tunneledRtspUrl = "";
            let playbackToken = "";
            if (response.ok) {
                const jsonResponse = await response.json();
                tunneledRtspUrl = jsonResponse.properties.streaming.rtspTunnelUrl;

                const responseAuth = await this.callApi(authUrl, 'POST');

                if (responseAuth.ok) {
                    const jsonAuthResponse = await responseAuth.json();
                    playbackToken = jsonAuthResponse.token;

                    return {
                        playbackToken: playbackToken,
                        tunneledRtspUrl: tunneledRtspUrl
                    };
                }
                else {
                    const errorMessageObj = await responseAuth.json();
                    throw new Error(`Cannot get video playback token: ${errorMessageObj.error.message}`);
                }
            }
            else {
                const errorMessageObj = await response.json();
                throw new Error(`Cannot get video playback url: ${errorMessageObj.error.message}`);
            }
        } catch (err) {
            throw err;
        }
    }
}

