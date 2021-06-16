import React, { Component } from 'react';

const armEndpoint = "management.azure.com";
const subscription = "86fe5e45-3696-4c0e-b88a-cf350e31ee68";
const resourceGroup = "client-web-app";
const accountName = "clientwebapp";
const apiVersion = "?api-version=2021-05-01-privatepreview";
const baseUrl = `https://${armEndpoint}/subscriptions/${subscription}/resourceGroups/${resourceGroup}/providers/Microsoft.Media/videoAnalyzers`;
const ioTHubDeviceId = "clientwebappdevice";
const ioTHubArmId = "/subscriptions/86fe5e45-3696-4c0e-b88a-cf350e31ee68/resourceGroups/client-web-app/providers/Microsoft.Devices/IotHubs/clientwebappiothub";
const ioTHubUserAssignedManagedIdentityArmId = "/subscriptions/86fe5e45-3696-4c0e-b88a-cf350e31ee68/resourceGroups/client-web-app/providers/Microsoft.ManagedIdentity/userAssignedIdentities/clientwebappiothubidentity";

export class FetchData extends Component {
    static displayName = FetchData.name;

    constructor(props) {
        super(props);
        this.state = {
            videoAnalyzers: [],
            pipelineTopologies: [],
            livePipelines: [],
            loading: true,
            behindProxy: false,
            livePipelineName: "",
            rtspUrl: "",
            rtspUsername: "",
            rtspPassword: "",
            livePipelineTopologyName: "",
            livePipelineState: "inactive"
        };
        this.token = null;
        this.deletePipelineTopology = this.deletePipelineTopologyOperation.bind(this);
        this.createPipelineTopology = this.createPipelineTopologyOperation.bind(this);
        this.createLivePipeline = this.createLivePipelineOperation.bind(this);
        this.deleteLivePipeline = this.deleteLivePipelineOperation.bind(this);
        this.changeStateLivePipeline = this.changeStateLivePipelineOperation.bind(this);
    }

    async componentDidMount() {
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

    async deleteLivePipelineOperation(livePipeline) {
        const token = this.token;
        const url = `${baseUrl}/${accountName}/livePipelines/${livePipeline}${apiVersion}`;
        try {
            await fetch(url, {
                method: 'DELETE',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            await this.getLivePipelines();
        }
        catch (e) {
            console.log(e);
        }
    }

    async deletePipelineTopologyOperation(pipelineTopologyName) {
        const token = this.token;
        const url = `${baseUrl}/${accountName}//pipelineTopologies/${pipelineTopologyName}${apiVersion}`;
        try {
            await fetch(url, {
                method: 'DELETE',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            await this.getPipelinesTopologies();
        }
        catch (e) {
            console.log(e);
        }
    }

    async createPipelineTopologyOperation(event) {
        event.preventDefault();
        const { pipelineTopologyName, videoName, behindProxy } = this.state;

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
                        "videoName": videoName,
                        "videoCreationProperties": {
                            "title": "Parking Lot (Camera 1)",
                            "description": "Parking lot south entrance",
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
                this.setState({ pipelineTopologyName: "", videoName: "", behindProxy: false });
                await this.getPipelinesTopologies();
            }
            else {
                alert("An error occurred, please check the console logs");
                console.log(response);
            }
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loading: false });
        }
    }

    async createLivePipelineOperation(event) {
        event.preventDefault();
        const { livePipelineName, rtspUrl, rtspUsername, rtspPassword, livePipelineTopologyName } = this.state;

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
                this.setState({ livePipelineName: "", rtspUrl: "", rtspUsername: "", rtspPassword: "", livePipelineTopologyName: "" });
                await this.getLivePipelines();
            }
            else {
                alert("An error occurred, please check the console logs");
                console.log(response);
            }
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loading: false });
        }
    }

    async changeStateLivePipelineOperation(livePipeline, state) {
        const token = this.token;
        const action = state === "inactive" ? "activate" : "deactivate";
        const url = `${baseUrl}/${accountName}/livePipelines/${livePipeline}/${action}${apiVersion}`;
        try {
            await fetch(url, {
                method: 'POST',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            await this.getLivePipelines();
        }
        catch (e) {
            console.log(e);
        }
    }

    async getPipelinesTopologies() {
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
                const jsonResponse = await response.json()
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
            this.setState({ loading: false });
        }
    }

    async getLivePipelines() {
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
                const jsonResponse = await response.json()
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
            this.setState({ loading: false });
        }
    }

    async listVideoAnalyzers() {
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
                const jsonResponse = await response.json()
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
        const value = event.target.type === "checkbox" ? event.target.checked : event.target.value;
        this.setState({
            ...this.state,
            [event.target.name] : value
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
                <form onSubmit={(e) => this.createPipelineTopology(e)}>
                    <fieldset>
                        <label>Behind proxy</label>&nbsp;<input type="checkbox" checked={this.state.behindProxy} name="behindProxy" onChange={(e) => this.setFormData(e)} />
                    </fieldset>
                    <fieldset>
                        <label>Name:</label>&nbsp;
                        <input name="pipelineTopologyName" value={this.state.pipelineTopologyName} onChange={(e) => this.setFormData(e)} />
                    </fieldset>
                    <fieldset >
                        <label>Video Name:</label>&nbsp;
                        <input name="videoName" value={this.state.videoName} onChange={(e) => this.setFormData(e)} />
                    </fieldset>
                    <button type="submit">Create</button>
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
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Description</th>
                            <th>Topology</th>
                            <th>State</th>
                            <th>Parameters</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {
                            livePipelines.map((data, index) =>
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
                                                <button className="btn btn-primary" onClick={() => this.changeStateLivePipeline(data.name, data.properties.state)}>Activate</button>
                                            )
                                            :
                                            (
                                                <button className="btn btn-primary" onClick={() => this.changeStateLivePipeline(data.name, data.properties.state)}>Deactivate</button>
                                            )
                                        }
                                    </td>
                                </tr>
                            )}
                    </tbody>
                </table>

                <h5>Add new</h5>
                <form onSubmit={(e) => this.createLivePipeline(e)}>
                    <fieldset>
                        <label>Name:</label>&nbsp;
                        <input name="livePipelineName" value={this.state.livePipelineName} onChange={(e) => this.setFormData(e)} />
                    </fieldset>
                    <fieldset >
                        <label>Video Name:</label>&nbsp;
                        <input name="livePipelineTopologyName" value={this.state.livePipelineTopologyName} onChange={(e) => this.setFormData(e)} />
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
                    <button type="submit">Create</button>
                </form>
            </div>
        );
    }

    render() {
        let videoAnalyzers = this.state.loading
            ? <p><em>Loading Video Analyzers...</em></p>
            : this.renderVideoAnalyzers();

        let pipelineTopologies = this.state.loading
            ? <p><em>Loading PipelineTopologies... </em></p>
            : this.renderPipelineTopologies();

        let livePipelines = this.state.loading
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