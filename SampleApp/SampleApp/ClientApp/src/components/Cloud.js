import React, { Component } from 'react';

export class Cloud extends Component {
    static displayName = Cloud.name;

    constructor(props) {
        super(props);
        this.state = {
            videoAnalyzers: [],
            pipelineTopologies: [],
            livePipelines: [],
            loading: true,
            loadingPipelineTopologies: true,
            loadingLivePipelines: true,
            behindProxy: false,
            videoName: "",
            livePipelineName: "",
            rtspUrl: "",
            rtspUsername: "",
            rtspPassword: "",
            livePipelineTopologyName: "",
            livePipelineState: "inactive",
            livePipelineEnabled: false,
            pipelineTopologiesEnabled: false,
            appSettings: null
        };
        this.token = null;
        this.deletePipelineTopology = this.deletePipelineTopologyOperation.bind(this);
        this.createPipelineTopology = this.createPipelineTopologyOperation.bind(this);
        this.createLivePipeline = this.createLivePipelineOperation.bind(this);
        this.deleteLivePipeline = this.deleteLivePipelineOperation.bind(this);
        this.changeStateLivePipeline = this.changeStateLivePipelineOperation.bind(this);
    }

    async componentDidMount() {
        await this.getConfig();
        await this.getToken();
        await this.listVideoAnalyzers();
        await this.getPipelinesTopologies();
        await this.getLivePipelines();
    }

    async getToken() {
        const response = await fetch('Auth/GetToken', {
            method: 'GET'
        });

        this.token = await response.text();
    }

    async getConfig() {
        const response = await fetch('Auth/GetConfig', {
            method: 'GET'
        });

        const jsonResponse = await response.json();

        const settings = { ...jsonResponse, "baseUrl": `https://${jsonResponse.armEndpoint}/subscriptions/${jsonResponse.subscription}/resourceGroups/${jsonResponse.resourceGroup}/providers/Microsoft.Media/videoAnalyzers` };
        this.setState({ appSettings: settings });
    }

    async deleteLivePipelineOperation(livePipeline) {
        const { baseUrl, apiVersion, accountName } = this.state.appSettings;
        const token = this.token;
        const url = `${baseUrl}/${accountName}/livePipelines/${livePipeline}${apiVersion}`;
        try {
            const response = await fetch(url, {
                method: 'DELETE',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (response.ok) {
                await this.getLivePipelines();
            }
            else {
                const errorMessageObj = await response.json();
                alert(`Cannot delete livepipeline: ${errorMessageObj.error.message}`);
            }
        }
        catch (e) {
            console.log(e);
        }
    }

    async deleteVideoOperation(videoName) {
        const { baseUrl, apiVersion, accountName } = this.state.appSettings;
        const token = this.token;
        const url = `${baseUrl}/${accountName}/videos/${videoName}${apiVersion}`;
        try {
            const response = await fetch(url, {
                method: 'DELETE',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (!response.ok) {
                const errorMessageObj = await response.json();
                console.log(`Cannot delete video ${videoName}: ${errorMessageObj.error.message}`);
            }
        }
        catch (e) {
            console.log(e);
        }
    }

    async deletePipelineTopologyOperation(pipelineTopologyName) {
        const { baseUrl, apiVersion, accountName } = this.state.appSettings;
        const token = this.token;
        const url = `${baseUrl}/${accountName}/pipelineTopologies/${pipelineTopologyName}${apiVersion}`;
        try {
            const response = await fetch(url, {
                method: 'DELETE',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (response.ok) {
                await this.getPipelinesTopologies();
            }
            else {
                const errorMessageObj = await response.json();
                alert(`Cannot delete pipelineTopology: ${errorMessageObj.error.message}`);
            }
        }
        catch (e) {
            console.log(e);
        }
    }

    async createPipelineTopologyOperation(event) {
        event.preventDefault();
        const { pipelineTopologyName, behindProxy } = this.state;
        const { baseUrl, apiVersion, accountName, ioTHubDeviceId, ioTHubArmId, ioTHubUserAssignedManagedIdentityArmId } = this.state.appSettings;

        let body = {
            "Name": pipelineTopologyName,
            "Kind": "liveUltraLowLatency",
            "Sku": {
                "Name": "S1",
                "Tier": "Standard"
            },
            "Properties": {
                "description": "pipeline topology test description",
                "parameters": [
                    {
                        "name": "rtspUrlParameter",
                        "type": "String",
                        "description": "rtsp source url parameter"
                    },
                    {
                        "name": "rtspUsernameParameter",
                        "type": "String",
                        "description": "rtsp source username parameter"
                    },
                    {
                        "name": "rtspPasswordParameter",
                        "type": "SecretString",
                        "description": "rtsp source password parameter"
                    },
                    {
                        "name": "videoNameParameter",
                        "type": "String",
                        "description": "video name parameter"
                    }
                ],
                "sources": [
                    {
                        "@type": "#Microsoft.VideoAnalyzer.RtspSource",
                        "name": "rtspSource",
                        "transport": "tcp",
                        "endpoint": {
                            "@type": "#Microsoft.VideoAnalyzer.UnsecuredEndpoint",
                            "url": "${rtspUrlParameter}",
                            "credentials": {
                                "@type": "#Microsoft.VideoAnalyzer.UsernamePasswordCredentials",
                                "username": "${rtspUsernameParameter}",
                                "password": "${rtspPasswordParameter}"
                            }
                        }
                    }
                ],
                "sinks": [
                    {
                        "@type": "#Microsoft.VideoAnalyzer.VideoSink",
                        "name": "videoSink",
                        "videoName": "${videoNameParameter}",
                        "videoCreationProperties": {
                            "title": "Sample Video",
                            "description": "Sample Video",
                            "segmentLength": "PT30S"
                        },
                        "inputs": [
                            {
                                "nodeName": "rtspSource"
                            }
                        ]
                    }
                ]
            }
        };

        if (behindProxy) {
            let source = body.Properties.sources.pop();
            let endpoint = source.endpoint;
            source.endpoint = {
                ...endpoint, "tunnel": {
                    "@type": "#Microsoft.VideoAnalyzer.IotSecureDeviceRemoteTunnel",
                    "deviceId": ioTHubDeviceId,
                    "iotHubArmId": ioTHubArmId,
                    "userAssignedManagedIdentityArmId": ioTHubUserAssignedManagedIdentityArmId
                }
            };

            body.Properties.sources.push(source);
        }
       
        const token = this.token;
        const url = `${baseUrl}/${accountName}/pipelineTopologies/${pipelineTopologyName}${apiVersion}`;
        try {
            const response = await fetch(url, {
                method: 'PUT',
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(body)
            });

            if (response.ok) {
                this.setState({ pipelineTopologyName: "", videoName: "", behindProxy: false }, async () =>
                    await this.getPipelinesTopologies());
            }
            else {
                const errorMessageObj = await response.json();
                alert(`Cannot create tje pipelineTopology: ${errorMessageObj.error.message}`);
            }
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loadingPipelineTopologies: false });
        }
    }

    async createLivePipelineOperation(event) {
        event.preventDefault();
        const { livePipelineName, rtspUrl, rtspUsername, rtspPassword, livePipelineTopologyName, videoName } = this.state;
        const { baseUrl, accountName, apiVersion } = this.state.appSettings;

        let body = {
            "name": livePipelineName,
            "properties": {
                "topologyName": livePipelineTopologyName,
                "description": "live pipeline test description",
                "bitrateKbps": 500,
                "parameters": [
                    {
                        "name": "rtspUrlParameter",
                        "value": rtspUrl
                    },
                    {
                        "name": "rtspUsernameParameter",
                        "value": rtspUsername
                    },
                    {
                        "name": "rtspPasswordParameter",
                        "value": rtspPassword
                    },
                    {
                        "name": "videoNameParameter",
                        "value": videoName
                    }
                ]
            }
        }
        
        const token = this.token;
        const url = `${baseUrl}/${accountName}/livePipelines/${livePipelineName}${apiVersion}`;
        try {
            const response = await fetch(url, {
                method: 'PUT',
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(body)
            });

            if (response.ok) {
                this.setState({ livePipelineName: "", rtspUrl: "", rtspUsername: "", rtspPassword: "", livePipelineTopologyName: "", videoName: "" },
                    async() => await this.getLivePipelines());           
            }
            else {
                const errorMessageObj = await response.json();
                alert(`Cannot create livepipeline: ${errorMessageObj.error.message}`);
            }
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loadingLivePipelines: false });
        }
    }

    async checkStatus(asyncOpUrl) {
        const token = this.token;
        
        try {
            const asyncResponse = await fetch(asyncOpUrl, {
                method: 'GET',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

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

    async changeStateLivePipelineOperation(livePipeline, properties) {
        const { baseUrl, accountName, apiVersion } = this.state.appSettings;
        const token = this.token;
        const action = properties.state === "inactive" ? "activate" : "deactivate";
        
        const url = `${baseUrl}/${accountName}/livePipelines/${livePipeline}/${action}${apiVersion}`;
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (response.ok) {
                let asyncOpUrl = response.headers.get("azure-asyncoperation");
                const asyncResponse = await fetch(asyncOpUrl, {
                    method: 'GET',
                    headers: {
                        "Authorization": `Bearer ${token}`
                    }
                });

                if (asyncResponse.ok) {
                    let result = await this.checkStatus(asyncOpUrl);

                    if (result) {
                        await this.getLivePipelines();
                    }
                    else {
                        alert("Operation failed, please check the console log.");
                    }
                }
                else {
                    alert("Operation failed, please check the console log.");
                    console.log(await response.text());
                }
            }
            else {
                alert("Operation failed, please check the console log.");
                console.log(await response.text());
            }

            if (action === "deactivate") {
                this.deleteVideoPlayer(livePipeline);
                const videoName = properties.parameters.find(x => x.name === "videoNameParameter").value;
                this.deleteVideoOperation(videoName);
            }
        }
        catch (e) {
            console.log(e);
        }
    }

    async getPipelinesTopologies() {
        const { baseUrl, accountName, apiVersion } = this.state.appSettings;
        const token = this.token;
        const url = `${baseUrl}/${accountName}/pipelineTopologies${apiVersion}`;
        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            var data = [];

            if (response.ok) {
                const jsonResponse = await response.json();
                data = jsonResponse.value;
            }
            else {
                console.log(response.statusText);
            }

            this.setState({ pipelineTopologies: data });
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loadingPipelineTopologies: false });
        }
    }

    async getLivePipelines() {
        const { baseUrl, accountName, apiVersion } = this.state.appSettings;
        const token = this.token;
        const url = `${baseUrl}/${accountName}/livePipelines${apiVersion}`;
        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            var data = [];

            if (response.ok) {
                const jsonResponse = await response.json();
                data = jsonResponse.value;
            }
            else {
                console.log(response.statusText);
            }

            this.setState({ livePipelines: data });
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loadingLivePipelines: false });
        }
    }

    async getVideoPlayback(videoName, pipelineName) {
        const { baseUrl, accountName, apiVersion } = this.state.appSettings;
        const token = this.token;
        const url = `${baseUrl}/${accountName}/videos/${videoName}${apiVersion}`;
        const authUrl = `${baseUrl}/${accountName}/videos/${videoName}/listStreamingToken${apiVersion}`;
        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            let tunneledRtspUrl = "";
            let playbackToken = "";
            if (response.ok) {
                const jsonResponse = await response.json();
                tunneledRtspUrl = jsonResponse.properties.streaming.rtspTunnelUrl;

                const responseAuth = await fetch(authUrl, {
                    method: 'POST',
                    headers: {
                        "Authorization": `Bearer ${token}`
                    }
                });

                if (responseAuth.ok) {
                    const jsonAuthResponse = await responseAuth.json();
                    playbackToken = jsonAuthResponse.token;
                }
                else {
                    const errorMessageObj = await responseAuth.json();
                    alert(`Cannot get video playback token: ${errorMessageObj.error.message}`);
                }
            }
            else {
                const errorMessageObj = await response.json();
                alert(`Cannot get video playback url: ${errorMessageObj.error.message}`);
            }

            this.renderVideoPlayer(tunneledRtspUrl, playbackToken, pipelineName);
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loadingLivePipelines: false });
        }
    }

    async listVideoAnalyzers() {
        const { baseUrl, apiVersion } = this.state.appSettings;
        const token = this.token;
        const url = `${baseUrl}${apiVersion}`;
        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            var data = [];

            if (response.ok) {
                const jsonResponse = await response.json();
                data = jsonResponse.value;
            }
            else {
                console.log(response.statusText);
            }

            this.setState({ videoAnalyzers: data });
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loading: false });
        }
    }

    setFormData(event) {
        const elementType = event.target.parentElement.parentElement.name;
        const value = event.target.type === "checkbox" ? event.target.checked : event.target.value;
        this.setState({
            ...this.state,
            [event.target.name]: value
        }, () => this.validate(elementType));
    }

    validate(elementType) {
        const { livePipelineName, rtspUrl, rtspUsername, rtspPassword, livePipelineTopologyName, videoName, pipelineTopologyName } = this.state;

        let isLivePipelineValid = false;
        let isPipelineTopologiesValid = false;

        if (elementType === "livepipeline") {
            isLivePipelineValid = livePipelineName.length > 0 && rtspUrl.length > 0 && rtspUsername.length > 0 && rtspPassword.length > 0 && livePipelineTopologyName.length > 0 && videoName.length > 0;
        }
        else {
            isPipelineTopologiesValid = pipelineTopologyName.length;
        }

        this.setState({
            livePipelineEnabled: isLivePipelineValid,
            pipelineTopologiesEnabled: isPipelineTopologiesValid
        });
    }

    renderVideoAnalyzers() {
        const { videoAnalyzers } = this.state;
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Id</th>
                        <th>Location</th>
                        <th>Type</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        videoAnalyzers.map((data, index) =>
                            <tr key={index}>
                                <td>{data.name}</td>
                                <td>{data.id}</td>
                                <td>{data.location}</td>
                                <td>{data.type}</td>
                            </tr>
                        )}
                </tbody>
            </table>
        );
    }

    renderPipelineTopologies() {
        const { pipelineTopologies } = this.state;
        return (
            <div>
                <h3>Pipeline Topologies</h3>
                <table className='table table-striped' aria-labelledby="tabelLabel">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Description</th>
                            <th>Parameters</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {
                            pipelineTopologies.map((data, index) =>
                                <tr key={index}>
                                    <td>{data.name}</td>
                                    <td>{data.properties.description}</td>
                                    <td>
                                        <ul>
                                            {data.properties.parameters.map((p,i) =>
                                                <li key={i}>{p.name}</li>
                                            )}
                                        </ul>
                                    </td>
                                    <td>
                                        <button className="btn btn-primary" onClick={() => this.deletePipelineTopology(data.name)}>Delete</button>
                                    </td>
                                </tr>
                            )}
                    </tbody>
                </table>

                <h5>Add new</h5>
                <form name="pipelinetopology" onSubmit={(e) => this.createPipelineTopology(e)}>
                    <fieldset>
                        <label>Behind proxy</label>&nbsp;<input type="checkbox" checked={this.state.behindProxy} name="behindProxy" onChange={(e) => this.setFormData(e)} />
                    </fieldset>
                    <fieldset>
                        <label>Name:</label>&nbsp;
                        <input name="pipelineTopologyName" value={this.state.pipelineTopologyName} onChange={(e) => this.setFormData(e)} />
                    </fieldset>
                    <button type="submit" disabled={!this.state.pipelineTopologiesEnabled}>Create</button>
                </form>
            </div>
        );
    }

    renderLivePipelines() {
        const { livePipelines } = this.state;
        return (
            <div>
                <h3>LivePipelines</h3>
                <table className='table table-striped' aria-labelledby="tabelLabel">
                    <tbody>
                        {
                            livePipelines.map((data, index) =>
                                <div>
                                <tr>
                                    <th>Name</th>
                                    <th>Description</th>
                                    <th>Topology</th>
                                    <th>State</th>
                                    <th>Parameters</th>
                                    <th>Action</th>
                                </tr>
                                <tr key={index}>
                                    <td>{data.name}</td>
                                    <td>{data.properties.description}</td>
                                    <td>{data.properties.topologyName}</td>
                                    <td>{data.properties.state}</td>
                                    <td>
                                        <ul>
                                            {data.properties.parameters.map((p, i) =>
                                                <li key={i}>{p.name}: <b>{p.value === undefined ? "**********" : p.value}</b></li>
                                            )}
                                        </ul>
                                    </td>
                                    <td>
                                        <button className="btn btn-primary" onClick={() => this.deleteLivePipeline(data.name)}>Delete</button><br /><br />
                                        {
                                            data.properties.state === "inactive" ? (
                                                <button className="btn btn-primary" onClick={() => this.changeStateLivePipeline(data.name, data.properties)}>Activate</button>
                                            )
                                            :
                                            (
                                                <div>
                                                    <button className="btn btn-primary" onClick={() => this.changeStateLivePipeline(data.name, data.properties)}>Deactivate</button><br /><br />
                                                    <button className="btn btn-primary" onClick={() => this.getVideoPlayback(data.properties.parameters.find(x => x.name === "videoNameParameter").value, data.name)}>Play video</button>
                                                </div>
                                            )
                                        }
                                    </td>
                                    </tr>
                                    <tr>
                                        <td colSpan="6">
                                            <div>
                                                <div id={"videoRootContainer" + data.name}>
                                                    {/*lva-rtsp-player instances will be added here*/}
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr><td colSpan="6"></td></tr>
                                </div>
                            )}
                    </tbody>
                </table>
                <h5>Add new</h5>
                <form name="livepipeline" onSubmit={(e) => this.createLivePipeline(e)}>
                    <fieldset>
                        <label>Name:</label>&nbsp;
                        <input name="livePipelineName" value={this.state.livePipelineName} onChange={(e) => this.setFormData(e)} />
                    </fieldset>
                    <fieldset >
                        <label>Topology Name:</label>&nbsp;
                         <select name="livePipelineTopologyName" value={this.state.livePipelineTopologyName} onChange={(e) => this.setFormData(e)}>
                            <option value="">Select:</option>
                            {
                                this.state.pipelineTopologies.map((item, index) =>
                                    <option key={index} value={item.name}>{item.name}</option>
                                )
                            }
                        </select>
                    </fieldset>
                    <fieldset >
                        <label>rtsp Url:</label>&nbsp;
                        <input name="rtspUrl" value={this.state.rtspUrl} onChange={(e) => this.setFormData(e)} placeholder="rtsp://rtspsim:554/media/lots_015.mkv"/>
                    </fieldset>
                    <fieldset >
                        <label>rtsp Username:</label>&nbsp;
                        <input name="rtspUsername" value={this.state.rtspUsername} onChange={(e) => this.setFormData(e)} placeholder="username"/>
                    </fieldset>
                    <fieldset >
                        <label>rtsp Password:</label>&nbsp;
                        <input type="password" name="rtspPassword" value={this.state.rtspPassword} onChange={(e) => this.setFormData(e)} placeholder="*******"/>
                    </fieldset>
                    <fieldset >
                        <label>Video Name:</label>&nbsp;
                        <input name="videoName" value={this.state.videoName} onChange={(e) => this.setFormData(e)} placeholder="SampleVideo" />
                    </fieldset>
                    <button type="submit" disabled={!this.state.livePipelineEnabled}>Create</button>
                </form>
            </div>
        );
    }

    renderVideoPlayer(wsHost, websocketToken, pipelineName) {
        let videoId = 0;

        // Dynamically create and add instances of lva-rtsp-player based on input fields. A dummy value for rtspUri is required.
        const createVideo = (id, webSocketUri, authorizationToken) => {
            let player = document.createElement('lva-rtsp-player')
            player.id = "video" + id.toString();
            player.webSocketUri = webSocketUri;
            player.rtspUri = "rtsp://localhost:8554/test";
            player.style.width = "720px";
            player.style.height = "405px";
            player.authorizationToken = authorizationToken;
            let videoRootContainer = document.getElementById("videoRootContainer" + pipelineName);
            videoRootContainer.append(player);
        }

        createVideo(videoId++, wsHost, websocketToken);
    }

    deleteVideoPlayer(pipelineName) {
        let videoRootContainer = document.getElementById("videoRootContainer" + pipelineName);
        while (videoRootContainer.firstChild) {
            videoRootContainer.firstChild.remove();
        }
    }

    render() {
        let videoAnalyzers = this.state.loading
            ? <p><em>Loading Video Analyzers...</em></p>
            : this.renderVideoAnalyzers();

        let pipelineTopologies = this.state.loadingPipelineTopologies
            ? <p><em>Loading PipelineTopologies... </em></p>
            : this.renderPipelineTopologies();

        let livePipelines = this.state.loadingPipelineTopologies && this.state.loadingLivePipelines
            ? <p><em>Loading LivePipelines...</em></p>
            : this.renderLivePipelines();

        return (
            <div>
                <h1 id="tabelLabel" >Video Analyzers</h1>
                <p>This component demonstrates fetching Video Analyzers.</p>
                {videoAnalyzers}
                <hr />
                <br/>
                {pipelineTopologies}
                <hr />
                <br />
                {livePipelines}
                <hr />
                <br />
            </div>
        );
    }
}