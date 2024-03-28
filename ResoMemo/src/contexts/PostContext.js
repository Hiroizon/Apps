import React, { createContext, useState, useEffect } from 'react';
import { readPosts, writePosts } from '../utils/postUtils';

export const PostContext = createContext();

export const PostProvider = ({ children }) => {
    const [posts, setPosts] = useState([]);
    const [searchQuery, setSearchQuery] = useState('');
    const [selectedDate, setSelectedDate] = useState(null);

  useEffect(() => {
    const fetchPosts = async () => {
      const posts = await readPosts();
      setPosts(posts);
    };

    fetchPosts();
  }, []);

  const updatePost = (id, updatedPost) => {
    const updatedPosts = posts.map(post => (post.id === id ? updatedPost : post));
    setPosts(updatedPosts);
    writePosts(updatedPosts);
  };

  const deletePost = id => {
    const updatedPosts = posts.filter(post => post.id !== id);
    setPosts(updatedPosts);
    writePosts(updatedPosts);
  };

  const filteredPosts = posts.filter(post =>
    post.content.toLowerCase().includes(searchQuery.toLowerCase()) &&
    (!selectedDate || new Date(post.created_at).toDateString() === selectedDate.toDateString())
  );

  return (
    <PostContext.Provider value={{ posts: filteredPosts, setPosts, updatePost, deletePost, searchQuery, setSearchQuery, selectedDate, setSelectedDate }}>
      {children}
    </PostContext.Provider>
  );
};