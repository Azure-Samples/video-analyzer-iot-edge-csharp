import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Cloud } from './components/Cloud';
import { Edge } from './components/Edge';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
            <Route exact path='/' component={Cloud} />
            <Route exact path='/edge' component={Edge} />
      </Layout>
    );
  }
}
