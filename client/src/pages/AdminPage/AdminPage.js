import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container } from 'react-bootstrap';
import AdminPanel from '../../components/admin/AdminPanel';

const AdminPage = () => {
  const navigate = useNavigate();

  useEffect(() => {
    const checkAdminAccess = () => {
      const userData = JSON.parse(localStorage.getItem('currentUser') || 'null');
      
      if (!userData || userData.role !== 'Admin') {
        navigate('/'); 
      }
    };

    checkAdminAccess();
  }, [navigate]);

  return (
    <Container fluid className="p-0 admin-page-container">
      <AdminPanel />
    </Container>
  );
};

export default AdminPage;