import React, { useContext } from 'react';
import { PostContext } from '../contexts/PostContext';
import Post from './Post';

const Timeline = () => {
  const { posts, searchQuery, setSearchQuery, selectedDate, setSelectedDate } = useContext(PostContext);

  return (
    <div className="container">
      <input
        type="text"
        placeholder="Search posts..."
        value={searchQuery}
        onChange={e => setSearchQuery(e.target.value)}
      />
      <input
        type="date"
        value={selectedDate ? selectedDate.toISOString().slice(0, 10) : ''}
        onChange={e => setSelectedDate(e.target.value ? new Date(e.target.value) : null)}
      />
      {posts.map(post => (
        <Post key={post.id} post={post} />
      ))}
    </div>
  );
};

export default Timeline;