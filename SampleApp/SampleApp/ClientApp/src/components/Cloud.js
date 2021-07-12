﻿import React, { Component } from 'react';
import CloudApi from '../helpers/CloudApi';

const RtspDeviceIdParameter = "rtspDeviceIdParameter";

export class Cloud extends Component {
    static displayName = Cloud.name;

    constructor(props) {
        super(props);
        this.api = new CloudApi();
        this.api.init();

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
            deviceId: "",
            livePipelineTopologyName: "",
            livePipelineState: "inactive",
            livePipelineEnabled: false,
            pipelineTopologiesEnabled: false,
            appSettings: null,
            showDeviceId: false
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
        this.token = await this.api.getToken();
    }

    async getConfig() {
        const settings = await this.api.getConfig();
        this.setState({ appSettings: settings });
    }

    async deleteLivePipelineOperation(livePipeline) {
        try {
            await this.api.deleteLivePipeline(livePipeline);
            await this.getLivePipelines();
        }
        catch (e) {
            alert(`Cannot delete livepipeline: ${e}`);
        }
    }

    async deleteVideoOperation(videoName) {
        try {
            await this.api.deleteVideo(videoName);
        }
        catch (e) {
            alert(`Cannot delete video ${videoName}: ${e}`);
        }
    }

    async deletePipelineTopologyOperation(pipelineTopologyName) {
        try {
            await this.api.deletePipelineTopology(pipelineTopologyName);
            await this.getPipelinesTopologies();
        }
        catch (e) {
            alert(`Cannot delete pipelineTopology: ${e}`);
        }
    }

    async createPipelineTopologyOperation(event) {
        event.preventDefault();
        const { pipelineTopologyName, behindProxy } = this.state;
        const { ioTHubArmId, ioTHubUserAssignedManagedIdentityArmId } = this.state.appSettings;

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
            let parameters = body.Properties.parameters;
            const deviceIdParam = {
                "name": RtspDeviceIdParameter,
                "type": "String",
                "description": "device id parameter"
            }
            parameters.push(deviceIdParam);

            let source = body.Properties.sources.pop();
            let endpoint = source.endpoint;
            source.endpoint = {
                ...endpoint, "tunnel": {
                    "@type": "#Microsoft.VideoAnalyzer.IotSecureDeviceRemoteTunnel",
                    "deviceId": "${" + RtspDeviceIdParameter + "}",
                    "iotHubArmId": ioTHubArmId,
                    "userAssignedManagedIdentityArmId": ioTHubUserAssignedManagedIdentityArmId
                }
            };

            body.Properties.sources.push(source);
        }
       
        try {
            await this.api.createPipelineTopology(body);
            this.setState({ pipelineTopologyName: "", videoName: "", behindProxy: false }, async () =>
                await this.getPipelinesTopologies());
        }
        catch (e) {
            alert(`Cannot create the pipelineTopology: ${e}`);
        }
        finally {
            this.setState({ loadingPipelineTopologies: false });
        }
    }

    async createLivePipelineOperation(event) {
        event.preventDefault();
        const { livePipelineName, rtspUrl, rtspUsername, rtspPassword, livePipelineTopologyName, videoName, deviceId, showDeviceId } = this.state;

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

        if (showDeviceId && deviceId.length > 0) {
            const deviceParam = {
                "name": RtspDeviceIdParameter,
                "value": deviceId
            };

            body.properties.parameters.push(deviceParam);
        }

        try {
            await this.api.createLivePipeline(body);
            this.setState({ livePipelineName: "", rtspUrl: "", rtspUsername: "", rtspPassword: "", livePipelineTopologyName: "", videoName: "" },
                async () => await this.getLivePipelines());
        }
        catch (e) {
            alert(`Cannot create livepipeline: ${e}`);
        }
        finally {
            this.setState({ loadingLivePipelines: false });
        }
    }

    async changeStateLivePipelineOperation(livePipeline, properties) {
        try {
            await this.api.changeStateLivePipeline(livePipeline, properties);
            await this.getLivePipelines();

            if (properties.state !== "inactive") {
                this.deleteVideoPlayer(livePipeline);
            }
        }
        catch (e) {
            alert(e);
        }
    }

    async getPipelinesTopologies() {
        try {
            let data = await this.api.getPipelinesTopologies();
            this.setState({ pipelineTopologies: data });
        }
        catch (e) {
            alert(e);
        }
        finally {
            this.setState({ loadingPipelineTopologies: false });
        }
    }

    async getLivePipelines() {
        try {
            let data = await this.api.getLivePipelines();
            this.setState({ livePipelines: data });
        }
        catch (e) {
            alert(e);
        }
        finally {
            this.setState({ loadingLivePipelines: false });
        }
    }

    async getVideoPlayback(videoName, pipelineName) {
        
        try {
            let response = await this.api.getVideoPlayback(videoName);
            this.renderVideoPlayer(response.tunneledRtspUrl, response.playbackToken, pipelineName);
        }
        catch (e) {
            alert(e);
        }
    }

    async listVideoAnalyzers() {
        try {
            let data = await this.api.getVideoAnalyzers();
            this.setState({ videoAnalyzers: data });
        }
        catch (e) {
            alert(e);
        }
        finally {
            this.setState({ loading: false });
        }
    }

    setFormData(event) {
        const { pipelineTopologies, showDeviceId } = this.state;
        const elementType = event.target.parentElement.parentElement.name;
        const value = event.target.type === "checkbox" ? event.target.checked : event.target.value;
        let isBehindProxy = showDeviceId;

        if (event.target.type === "select-one" && value !== "") {
            const selectedPipelineTopology = pipelineTopologies.find(x => x.name === value);

            if (selectedPipelineTopology != null) {
                const result = selectedPipelineTopology.properties.parameters.find(x => x.name === RtspDeviceIdParameter);
                isBehindProxy = result != undefined;
            }
        }

        this.setState({
            ...this.state,
            [event.target.name]: value,
            showDeviceId: isBehindProxy
        }, () => this.validate(elementType));
    }

    validate(elementType) {
        const { livePipelineName, rtspUrl, rtspUsername, rtspPassword, livePipelineTopologyName, videoName, pipelineTopologyName, showDeviceId, deviceId } = this.state;

        let isLivePipelineValid = false;
        let isPipelineTopologiesValid = false;

        if (elementType === "livepipeline") {
            isLivePipelineValid = livePipelineName.length > 0 && rtspUrl.length > 0 && rtspUsername.length > 0 && rtspPassword.length > 0 && livePipelineTopologyName.length > 0 && videoName.length > 0;

            if (showDeviceId) {
                isLivePipelineValid = isLivePipelineValid && deviceId.length > 0;
            }
        }
        else {
            isPipelineTopologiesValid = pipelineTopologyName !== undefined && pipelineTopologyName.length > 0;
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
                    <fieldset>
                        <label>Name:</label>&nbsp;
                        <input name="livePipelineName" value={this.state.livePipelineName} onChange={(e) => this.setFormData(e)} />
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
                    {
                        this.state.showDeviceId ?
                        <fieldset>
                            <label>Device Id:</label>&nbsp;
                        <input name="deviceId" value={this.state.deviceId} onChange={(e) => this.setFormData(e)} placeholder="Camera01" />
                            </fieldset>
                            :
                            null
                    }
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