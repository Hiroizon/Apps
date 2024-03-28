import React, { useState, useContext } from 'react';
import { PostContext } from '../contexts/PostContext';
import { readPosts, writePosts } from '../utils/postUtils';

const PostForm = () => {
  const [content, setContent] = useState('');
  const [imagePath, setImagePath] = useState('');
  const { posts, setPosts } = useContext(PostContext);

  const handleSubmit = e => {
    e.preventDefault();

    const newPost = {
      id: Date.now(),
      content,
      image_path: imagePath,
      created_at: new Date().toISOString(),
    };

    writePosts([...posts, newPost]);
    setPosts([...posts, newPost]);
    setContent('');
    setImagePath('');
  };

  return (
    <div className="post-form">
      <textarea
        placeholder="What's happening?"
        value={content}
        onChange={e => setContent(e.target.value)}
      />
      <div className="post-form-actions">
        <input
          type="text"
          placeholder="Image URL (optional)"
          value={imagePath}
          onChange={e => setImagePath(e.target.value)}
        />
        <button type="submit" onClick={handleSubmit}>
          Post
        </button>
      </div>
    </div>
  );
};

export default PostForm;