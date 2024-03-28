import React from 'react';
import Timeline from './Timeline';
import PostForm from './PostForm';
import { PostProvider } from '../contexts/PostContext';

const App = () => {
  return (
    <PostProvider>
      <div className="container">
        <h1>ResoMemo</h1>
        <PostForm />
        <Timeline />
      </div>
    </PostProvider>
  );
};

export default App;