import React, { useState, useContext } from 'react';
import { PostContext } from '../contexts/PostContext';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPencilAlt, faTrashAlt } from '@fortawesome/free-solid-svg-icons';

const Post = ({ post }) => {
  const [isEditing, setIsEditing] = useState(false);
  const [content, setContent] = useState(post.content);
  const [imagePath, setImagePath] = useState(post.image_path);
  const { updatePost, deletePost } = useContext(PostContext);

  const handleSave = () => {
    updatePost(post.id, { ...post, content, image_path: imagePath });
    setIsEditing(false);
  };

  const handleDelete = () => {
    deletePost(post.id);
  };

  return (
    <div className="post">
      <div className="post-actions">
        <FontAwesomeIcon icon={faPencilAlt} onClick={() => setIsEditing(true)} />
        <FontAwesomeIcon icon={faTrashAlt} onClick={handleDelete} />
      </div>
      {isEditing ? (
        <>
          <textarea
            value={content}
            onChange={e => setContent(e.target.value)}
          />
          <input
            type="text"
            value={imagePath}
            onChange={e => setImagePath(e.target.value)}
          />
          <button onClick={handleSave}>Save</button>
        </>
      ) : (
        <>
          <p className="post-content">{content.split('\n').map((line, i) => (
            <React.Fragment key={i}>
              {line}
              <br />
            </React.Fragment>
          ))}</p>
          {post.image_path && <img src={post.image_path} alt="Post" />}
          <p className="date">{new Date(post.created_at).toLocaleString()}</p>
        </>
      )}
    </div>
  );
};

export default Post;